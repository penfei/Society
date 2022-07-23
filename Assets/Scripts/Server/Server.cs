using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Timers;
using Society.Characters;
using Society.Mapping;
using UnityEngine;

namespace Society.Server
{
    public class Server
    {
        private static readonly int HOURS_IN_DAY = 24;
        private static readonly int CHARACTERS_PER_TICK = 1000;
        private static readonly int SERVER_TICK = 33;

        public Map Map { get { return _map; } }
        public List<Character> Characters { get { return _characters; } }
        public float HourDuration { get { return _hourDuration; } }
        public Type[] Actions { get { return _actions; } }

        private Map _map = new Map();
        private List<Character> _characters = new List<Character>();
        private SortedList<int, Character> _needActionCharacters = new SortedList<int, Character>(new ByPriority());
        //private List<Character> _needActionCharacters = new List<Character>();

        private int _dayDuration = 216000;
        private int _dayCount = 0;
        private float _hourDuration;
        private Type[] _actions;

        private Timer _dayTimer;
        private Timer _tickTimer;
        private ByPriority _comparer = new ByPriority();

        public void Init(string mapPath)
        {
            _hourDuration = _dayDuration / HOURS_IN_DAY;

            Load(mapPath);

            _actions = GetDerivedTypes(typeof(BaseAction));

            CreateCharacters();
        }

        public void AddCharacterToQueue(Character character)
        {
            _needActionCharacters.Add(character.Priority, character);
            //_needActionCharacters.Add(character);
        }

        private void Load(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
            {
                var header = reader.ReadInt32();
                _map.Load(reader, header);
            }
        }

        private void CreateCharacters()
        {
            foreach (var t in _map.Cells)
            {
                if (!t.IsUnderwater)
                {
                    var list = CharacterFactory.CreateCharacters(this, 100);
                    _characters.AddRange(list);
                }
            }

            _tickTimer = new Timer(SERVER_TICK);
            _tickTimer.Elapsed += Create;
            _tickTimer.Start();
        }

        private void Create(object sender, ElapsedEventArgs e)
        {
            var list = _needActionCharacters.Values.Take(CHARACTERS_PER_TICK);
            foreach (var c in list)
            {
                c.FindAction();
                _needActionCharacters.RemoveAt(0);
            }
            //var length = _needActionCharacters.Count > CHARACTERS_PER_TICK ? CHARACTERS_PER_TICK : _needActionCharacters.Count;

            //for (int i = 0; i < length; i++)
            //{
            //    _needActionCharacters[i].FindAction();
            //}

            //_needActionCharacters.RemoveRange(0, length);
            
            if (_needActionCharacters.Count == 0)
            {
                _tickTimer.Stop();
                _tickTimer.Dispose();

                Start();
            }
        }

        private void Start()
        {
            Debug.Log("Start. Characters - " + _characters.Count);

            _dayTimer = new Timer(_dayDuration);
            _dayTimer.Elapsed += NewDay;
            _dayTimer.Start();

            _tickTimer = new Timer(SERVER_TICK);
            _tickTimer.Elapsed += Tick;
            _tickTimer.Start();

            foreach(var c in _characters)
            {
                c.StartAction();
            }
        }

        private void Tick(object sender, ElapsedEventArgs e)
        {
            Debug.Log(_needActionCharacters.Count);
            var list = _needActionCharacters.Values.Take(CHARACTERS_PER_TICK);
            foreach (var c in list)
            {
                c.FindAction();
                c.StartAction();
                _needActionCharacters.RemoveAt(0);
            }

            //var length = _needActionCharacters.Count > CHARACTERS_PER_TICK ? CHARACTERS_PER_TICK : _needActionCharacters.Count;

            //for (int i = 0; i < length; i++)
            //{
            //    _needActionCharacters[i].FindAction();
            //    _needActionCharacters[i].StartAction();
            //}

            //_needActionCharacters.RemoveRange(0, length);
            //_needActionCharacters.Sort(_comparer);
        }

        private void NewDay(object sender, ElapsedEventArgs e)
        {
            _dayCount++;
        }

        private Type[] GetDerivedTypes(Type baseType)
        {
            List<Type> types = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                try
                {
                    types.AddRange(assembly.GetTypes().Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t)).ToArray());
                }
                catch (ReflectionTypeLoadException) { }
            }
            return types.ToArray();
        }
    }
}
