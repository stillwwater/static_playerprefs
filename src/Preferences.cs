using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public struct VariablePairInfo
    {
        public string key;
        public string value;
        public int line_number;
    }

    public struct TableInfo
    {
        public List<VariablePairInfo> pairs;
        public int line_number;
    }

    public class Preferences
    {
        Dictionary<string, TableInfo> tables;
        string source_filepath;

        public Preferences(string filepath) {
            source_filepath = filepath;
        }

        public void Load() {
            tables = new Dictionary<string, TableInfo>();
            int line_number = 0;
            string current_table_name = null;

            foreach (var raw_line in File.ReadAllLines(source_filepath)) {
                string line = raw_line.TrimStart();
                int comment = line.IndexOf('#');
                line_number++;

                if (comment >= 0) {
                    // Strip comments, including inline comments
                    line = line.Substring(0, comment);
                }

                if (line == "") continue;

                if (line[0] == ':') {
                    // Parse new table
                    current_table_name = line.Substring(1).Trim();

                    var table = new TableInfo() {
                        pairs = new List<VariablePairInfo>(),
                        line_number = line_number
                    };

                    if (tables.ContainsKey(current_table_name)) {
                        Error(line_number, "Duplicate table '{0}'", current_table_name);
                        continue;
                    }

                    tables.Add(current_table_name, table);
                } else if (current_table_name != null) {
                    // Parse key value pair
                    int space = line.IndexOf(' ');

                    if (space < 0) {
                        Error(line_number, "Key without a value");
                        continue;
                    }

                    var pair = new VariablePairInfo() {
                        key = line.Substring(0, space),
                        value = line.Substring(space).Trim(),
                        line_number = line_number
                    };

                    tables[current_table_name].pairs.Add(pair);
                } else {
                    Error(line_number, "Key value pair defined outside of a table definition");
                }
            }
        }

        public void Save() {
            var buffer = new StringBuilder();

            foreach (var table_data in tables) {
                buffer.Append(':');
                buffer.AppendLine(table_data.Key);

                foreach (var pair in table_data.Value.pairs) {
                    buffer.Append(pair.key);
                    buffer.Append(' ');
                    buffer.AppendLine(pair.value);
                }
            }

            File.WriteAllText(source_filepath, buffer.ToString());
            tables.Clear();
        }

        public object ReadTable(object value, string name) {
            var type = value.GetType();

            if (name == null) {
                name = type.Name;
            }

            if (!tables.ContainsKey(name)) {
                Debug.LogErrorFormat("Error: No table named {0}, perhaps it has already been loaded and destroyed.", name);
                return value;
            }

            var table = tables[name];
            tables.Remove(name);

            foreach (var pair in table.pairs) {
                var member      = type.GetField(pair.key);
                var member_type = member.FieldType;

                if (member_type == typeof(int)) {
                    int parsed_int;

                    if (int.TryParse(pair.value, out parsed_int)) {
                        member.SetValue(value, parsed_int);
                    } else {
                        Error(pair.line_number, "Expected an int value");
                    }
                } else if (member_type == typeof(float)) {
                    float parsed_float;

                    if (float.TryParse(pair.value, out parsed_float)) {
                        member.SetValue(value, parsed_float);
                    } else {
                        Error(pair.line_number, "Expected a float value");
                    }
                } else if (member_type == typeof(bool)) {
                    if (pair.value[0] == 't' || pair.value[0] == 'T') {
                        member.SetValue(value, true);
                    } else if (pair.value[0] == 'f' || pair.value[0] == 'F') {
                        member.SetValue(value, false);
                    } else {
                        Error(pair.line_number, "Expected a bool value");
                    }
                } else if (member_type == typeof(string)) {
                    member.SetValue(value, pair.value);
                } else {
                    Error(pair.line_number, "The type '{0}' is not supported by preferences, ", member_type);
                }
            }

            return value;
        }

        public object ReadTable(object value) {
            return ReadTable(value, null);
        }

        public void WriteTable(object value, string name) {
            var type = value.GetType();

            if (name == null) {
                name = type.Name;
            }

            var table = new TableInfo() {
                pairs = new List<VariablePairInfo>(),
            };

            foreach (var member in type.GetFields()) {
                // Save each member of value as a key, value pair
                table.pairs.Add(new VariablePairInfo() {
                    key = member.Name,
                    value = member.GetValue(value).ToString()
                });
            }

            if (tables.ContainsKey(name)) {
                Debug.LogWarningFormat("Performance Warning: Table {0} is already queued, call Save before re-writing table data.", name);
                tables[name] = table;
            } else {
                tables.Add(name, table);
            }
        }

        public void WriteTable(object value) {
            WriteTable(value, null);
        }

        void Error(int line_number, string format, params object[] args) {
            string message = string.Format(format, args);
            Debug.LogErrorFormat("Error: {0} in '{1}' at line {2}.", message, source_filepath, line_number);
        }
    }
}
