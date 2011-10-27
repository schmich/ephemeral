using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ephemeral
{
    interface ICommandHistory
    {
        void AddCommand(string command);

        IBidirectionalEnumerator<string> GetEnumerator();
    }

    class MemoryCommandHistory : ICommandHistory
    {
        public void AddCommand(string command)
        {
            _commandHistory.Insert(0, command);
        }

        public IBidirectionalEnumerator<string> GetEnumerator()
        {
            return new ListBidirectionalEnumerator<string>(_commandHistory);
        }

        List<string> _commandHistory = new List<string>();
    }

    interface IBidirectionalEnumerator<T> : IEnumerator<T>
    {
        bool MovePrev();
    }

    class ListBidirectionalEnumerator<T> : IBidirectionalEnumerator<T>
    {
        public ListBidirectionalEnumerator(IList<T> list)
        {
            _list = list;
            Reset();
        }

        public T Current
        {
            get
            {
                if (_position < 0 || _position >= _list.Count)
                    throw new InvalidOperationException();

                return _list[_position];
            }
        }

        public void Dispose()
        {
        }

        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            return (_position != _list.Count)
                && (++_position < _list.Count);
        }

        public bool MovePrev()
        {
            return (_position != (-1))
                && (--_position >= 0);
        }

        public void Reset()
        {
            _position = -1;
        }

        int _position;
        IList<T> _list;
    }
}
