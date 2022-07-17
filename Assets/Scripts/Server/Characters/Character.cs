using System;
using System.Collections.Generic;

namespace Society.Characters
{
    public class Character
    {
        public int Priority { get { return _priority; } }
        public DateTime ActionResponseTime { get { return _actionResponseTime; } }

        public HexCell Location;

        private BaseAction _currentAction;
        private System.Random _random = new System.Random();

        private Society.Server.Server _server;
        private int _priority;
        private DateTime _actionResponseTime;
        private Type[] _actions;

        public Character(Society.Server.Server server)
        {
            _server = server;
            _actions = server.Actions;

            NeedAction();
        }

        public void NeedAction()
        {
            _priority = _random.Next(1, 3);
            _actionResponseTime = DateTime.Now;

            _server.AddCharacterToQueue(this);
        }

        public void FindAction()
        {
            _currentAction = (BaseAction)Activator.CreateInstance(_actions[_random.Next(0, _actions.Length)]);
        }

        public void StartAction()
        {
            _currentAction.OnActionEnd += EndAction;
            _currentAction.Start(_server.HourDuration);
        }

        public void EndAction()
        {
            _currentAction.OnActionEnd -= EndAction;
            _currentAction = null;

            NeedAction();
        }
    }

    public class ByPriority : IComparer<int>
    {
        public int Compare(int ch1, int ch2)
        {
            var result = ch2 - ch1;

            return result == 0 ? 1 : result;
        }
    }
}
