using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Timers;
using Society.Characters;
using Society.Mapping;

namespace Society.Server
{
    public class Server
    {
        private static readonly int HOURS_IN_DAY = 24;
        private static readonly int CHARACTERS_PER_TICK = 1000;

        public Map Map { get { return _map; } }
        public List<Character> Characters { get { return _characters; } }
        public float HourDuration { get { return _hourDuration; } }
        public Type[] Actions { get { return _actions; } }

        private Map _map = new Map();
        private List<Character> _characters = new List<Character>();
        private SortedList<int, Character> _needActionCharacters = new SortedList<int, Character>(new ByPriority());

        private int _dayDuration = 216000;
        private int _dayCount = 0;
        private float _hourDuration;
        private int _serverTick = 33;
        private Type[] _actions;

        private Timer _dayTimer;
        private Timer _tickTimer;

        public void Init(string mapPath)
        {
            _hourDuration = _dayDuration / HOURS_IN_DAY;

            Load(mapPath);

            _actions = GetDerivedTypes(typeof(BaseAction));

            foreach (var t in _map.Cells)
            {
                if (!t.IsUnderwater)
                {
                    var list = CharacterFactory.CreateCharacters(this, 100);
                    _characters.AddRange(list);
                }
            }

            _dayTimer = new Timer(_dayDuration);
            _dayTimer.Elapsed += NewDay;
            _dayTimer.Start();

            _tickTimer = new Timer(_serverTick);
            _tickTimer.Elapsed += Tick;
            _tickTimer.Start();
        }

        public void AddCharacterToQueue(Character character)
        {
            _needActionCharacters.Add(character.Priority, character);
        }

        private void NewDay(object sender, ElapsedEventArgs e)
        {
            _dayCount++;
        }

        private void Tick(object sender, ElapsedEventArgs e)
        {
            var list = _needActionCharacters.Values.Take(CHARACTERS_PER_TICK);
            foreach (var c in list)
            {
                c.FindAction();
                c.StartAction();
                _needActionCharacters.RemoveAt(0);
            }
        }

        private void Load(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
            {
                var header = reader.ReadInt32();
                _map.Load(reader, header);
            }
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
