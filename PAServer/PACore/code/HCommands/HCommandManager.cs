namespace PolyArchitect.Core {
    using CircularBuffer;
    using System;
    using System.Collections.Generic;

    public class HCommandStack {
        public const int MAX_COMMANDS = 64;

        private CircularBuffer<IHCommand> history = new(MAX_COMMANDS);

        // NOTE: The circular buffer will ensure the redo history stack will never grow larer
        //       than the max commands count.
        private Stack<IHCommand> redoHistory = new(MAX_COMMANDS);

        public void Do(IHCommand command) {
            redoHistory.Clear();

            command.Apply();
            history.PushBack(command);
        }

        public void Undo() {
            if (history.Size == 0) {
                throw new Exception(); // TODO: Handle invalid undo request
            }

            IHCommand command = history.PopBack();
            command.Undo();
            redoHistory.Push(command);
        }

        public void Redo() {
            if (redoHistory.Count == 0) {
                throw new Exception(); // TODO: Handle invalid redo request
            }

            IHCommand command = redoHistory.Pop();
            command.Apply();
            history.PushBack(command);
        }

        public void Clear() {
            history.Clear();
            redoHistory.Clear();
        }
    }
}