using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ephemeral
{
    interface CommandHistory
    {
        void AddCommand(string command);

        BidirectionalEnumerator<string> GetEnumerator();
    }

    class MemoryCommandHistory : CommandHistory
    {
        public void AddCommand(string command)
        {
            _commandHistory.Insert(0, command);
        }

        public BidirectionalEnumerator<string> GetEnumerator()
        {
            return new ListBidirectionalEnumerator<string>(_commandHistory);
        }

        List<string> _commandHistory = new List<string>();
    }

    interface BidirectionalEnumerator<T> : IEnumerator<T>
    {
        bool MovePrev();
    }

    class ListBidirectionalEnumerator<T> : BidirectionalEnumerator<T>
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
