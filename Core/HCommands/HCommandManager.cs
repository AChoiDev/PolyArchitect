namespace PolyArchitect.Core {
    using CircularBuffer;
    using System;
    using System.Collections.Generic;

    // stack implementation of command manager
    public class HCommandManagerStack {
        public const int MAX_COMMANDS = 50;

        private Stack<IHCommand> history = new(MAX_COMMANDS);
        private Stack<IHCommand> redoHistory = new(MAX_COMMANDS);

        public void Do(IHCommand command) {
            redoHistory.Clear();

            // NOTE: The circular buffer implementation avoids the expensive copying of values
            //       needed to limt the stack size.
            if (history.Count >= MAX_COMMANDS) {
                IHCommand[] commands = history.ToArray();
                history = new Stack<IHCommand>(commands[1..]);
            }

            command.Apply();
            history.Push(command);
        }

        public void Undo() {
            if (history.Count == 0) {
                throw new Exception(); // TODO: Handle invalid undo request
            }

            IHCommand command = history.Pop();
            command.Undo();
            redoHistory.Push(command);
        }

        public void Redo() {
            if (redoHistory.Count == 0) {
                throw new Exception(); // TODO: Handle invalid redo request
            }

            IHCommand command = redoHistory.Pop();
            command.Apply();

            // NOTE: Max command limit doesn't need to be checked as it was already checked during the initial applying of the command
            //       and the only way the check can be invalidated is if another command is performed which will clear the redo history
            //       in turn not allowing this code to even be run.
            history.Push(command);
        }

        public void Clear() {
            history.Clear();
            redoHistory.Clear();
        }
    }


    // circular buffer implementation of command manager
    // TODO: Verify with unit tests (when finally added) that this actually works before using.
    public class HCommandManagerCircularBuffer {
        public const int MAX_COMMANDS = 50;

        private CircularBuffer<IHCommand> history = new(MAX_COMMANDS);
        private int redoOffset = 0;

        public void Do(IHCommand command) {
            command.Apply();
            
            for (int i = 0; i < redoOffset; i++) {
                history.PopBack();
            }
            redoOffset = 0;

            history.PushBack(command);
        }

        public void Undo() {
            if (history.Size <= redoOffset) {
                throw new Exception(); // TODO: Handle invalid undo request
            }

            IHCommand back = history.Back();
            back.Undo();

            redoOffset = Math.Min(redoOffset + 1, MAX_COMMANDS);
        }

        public void Redo() {
            if (redoOffset == 0) {
                throw new Exception(); // TODO: Handle invalid redo request
            }
            redoOffset -= 1;

            int commandIndex = history.End - redoOffset;
            commandIndex = history.LoopIndex(commandIndex);

            IHCommand command = history[commandIndex];
            command.Apply();
        }

        public void Clear() {
            history.Clear();
            redoOffset = 0;
        }
    }
}