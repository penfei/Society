using Society.Characters;
using Society.Server;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UnityServer : MonoBehaviour
{
    private const int mapFileVersion = 5;

    public string Map;
    public HexGrid HexGrid;

    private Server _server = new Server();

    void Start()
    {
        Load(Path.Combine(Application.dataPath, Map + ".map"));

        _server.Init(Path.Combine(Application.dataPath, Map + ".map"));

        //Character ch = new Character();
        //ch.FindAction();
    }

    private void Load(string path)
    {
        using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
        {
            int header = reader.ReadInt32();
            if (header <= mapFileVersion)
            {
                HexGrid.Load(reader, header);
                HexMapCamera.ValidatePosition();
            }
            else
            {
                Debug.LogWarning("Unknown map format " + header);
            }
        }
    }

    
}
