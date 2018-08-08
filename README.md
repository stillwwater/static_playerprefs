
Static PlayerPrefs
==================

A simple system for loading and saving text files into a game, in such a way that the structure of the text file mirrors the structure of the source code.

*main.save*
```ruby
:WorldData
name    Main
level   12
time    69105.0
raining true
```

*world.cs*
```csharp
struct WorldData
{
    string name;
    int level;
    float time;
    bool raining;
}

Preferences savegame;
WorldData world;

void Awake() {
    savegame = Preferences.Load("main.save");
}

void Start() {
    world = (WorldData)savegame.ReadTable(new WorldData());
    print(world.time)  // 69105.0
}
```

[Complete Example](./examples)
