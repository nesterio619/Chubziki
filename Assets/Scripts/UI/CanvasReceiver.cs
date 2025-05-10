using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public abstract class CanvasReceiver : System.IDisposable
    {
        public abstract GameObject Canvas { get; }

        private readonly List<CanvasCommand> _commands = new List<CanvasCommand>();

        public void RegisterCanvasCommand(CanvasCommand command)
        {
            if (!command.IsDisposed && !_commands.Contains(command))
                _commands.Add(command);
        }
        public void UnregisterCanvasCommand(CanvasCommand command)
        {
            if (!command.IsDisposed && _commands.Contains(command))
                _commands.Remove(command);
        }

        public void ForceUpdateCommands()
        {
            foreach (var command in _commands)
                command.Update();
        }

        public void PurgeCommands()
        {
            if (_commands != null && _commands.Count > 0)
                while (_commands.Count > 0)
                {
                    var command = _commands[0];
                    if (!command.IsDisposed)
                        command.Dispose();
                }
        }

        public virtual void Dispose()
        {
            PurgeCommands();
        }
    }

}

