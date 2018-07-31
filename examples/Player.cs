///
/// Example of how to use Preferences.cs as a savegame
/// to store player data.
///
/// Using Preferences to load and save game data, this could be a savegame, config file etc...
///
/// - Create a Preferences object, giving it a file path to load and save data from;
///
/// - Load the file using Preferences.Load();
///
/// - Read tables using Preferences.ReadTable(object value), this will load data into
///    the object passed and return the object;
///
/// - Write the modified data by once again passing the object that should be saved;
///    using Preferences.WriteTable(object value);
///
/// - Save the file to disk using Preferences.Save().
///

using UnityEngine;

namespace Framework
{
    public struct PlayerData
    {
        public string name;
        public int level;
        public int experience;
    }

    public struct WorldData
    {
        public float time;
        public bool is_raining;
    }

    public class Player : MonoBehaviour
    {
        Preferences savegame;
        PlayerData player_data;
        WorldData world_data;

        void Awake() {
            savegame = new Preferences("example.save"); // Here example.save is in the root of the project.
            // Load savegame from disk.
            // Normally Loading and Saving this file is done in a GameManager,
            // with other entities reading and writing only the data they need.
            savegame.Load();

            Debug.Log("Loaded savegame");
        }

        void Start() {
            player_data = new PlayerData() {
                // Initialize name variable for player_data,
                // if it's not defined in the savegame file this value will not be changed.
                name = "Unnamed"
            };

            // Load values in the save game data into fields of the passed object.
            // ReadTable can only be called after the file has been loaded (with savegame.Load()), it also may only
            // be called once per type/table since the original file data is deleted from memory after it's read.

            player_data = (PlayerData)savegame.ReadTable(player_data); // Name in file is implied from type "PlayerData".

            Debug.Log("---------------- PlayerData");
            Debug.LogFormat("Loaded: {0} {1}", "name", player_data.name);
            Debug.LogFormat("Loaded: {0} {1}", "level", player_data.level);
            Debug.LogFormat("Loaded: {0} {1}", "experience", player_data.experience);

            world_data  = (WorldData)savegame.ReadTable(new WorldData(), "World"); // Use different name for WorldData struct.

            Debug.Log("---------------- WorldData");
            Debug.LogFormat("Loaded: {0} {1}", "time", world_data.time);
            Debug.LogFormat("Loaded: {0} {1}", "is_raining", world_data.is_raining);

        }

        void Update() {
            // Modify world_data
            world_data.time += Time.deltaTime;
        }

        void OnApplicationQuit() {
            // Write changes made to player_data and world_data.
            // This only temporarily writes the changes to memory before they are saved to disk.
            savegame.WriteTable(player_data);
            Debug.Log("Written: PlayerData");

            savegame.WriteTable(world_data);
            Debug.Log("Written: WorldData");

            // Save changes to disk.
            savegame.Save();
            Debug.Log("Saved savegame");
        }
    }
}
