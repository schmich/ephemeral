using System;
using System.Collections.Generic;
using System.Linq;
using Ephemeral.Commands;

namespace Ephemeral
{
    interface ICommandProvider
    {
        IEnumerable<Command> GetSuggestions(string input);
    }

    class CommandPriortyComparer : IComparer<Command>
    {
        public CommandPriortyComparer(string input)
        {
            _input = input;
        }

        public int Compare(Command p, Command q)
        {
            return GetPriority(p) - GetPriority(q);   
        }

        int GetPriority(Command c)
        {
            string name = c.Name;

            if (StringComparer.Ordinal.Equals(name, _input))
                return 0;

            if (StringComparer.OrdinalIgnoreCase.Equals(name, _input))
                return 10;

            string acronym = new string(name.Where(s => char.IsUpper(s)).ToArray());

            if (StringComparer.OrdinalIgnoreCase.Equals(acronym, _input))
                return 200;
            
            int index = name.IndexOf(_input, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
                return 300 + index;

            return int.MaxValue;
        }

        string _input;
    }

    class PatternCommandProvider : ICommandProvider
    {
        public PatternCommandProvider(CommandControllerEvents controller)
        {
            controller.CommandAdded += new Action<Command>(OnCommandAdded);
            controller.CommandRemoved += new Action<Command>(OnCommandRemoved);
        }

        void OnCommandAdded(Command command)
        {
            _indexer.AddCommand(command);
        }

        void OnCommandRemoved(Command command)
        {
            _indexer.RemoveCommand(command);
        }

        public IEnumerable<Command> GetSuggestions(string input)
        {
            List<Command> answer = new List<Command>(_indexer.GetMatches(input));
            answer.Sort(new CommandPriortyComparer(input));

            if ((answer.Count() == 0) && (_lastAnswer.Count() == 1) && (input.Length >= _lastInput.Length))
            {
                _lastInput = input;
                return _lastAnswer;
            }
            else
            {
                _lastInput = input;
                _lastAnswer = answer;
                return answer;
            }
        }

        Indexer _indexer = new Indexer();

        string _lastInput = string.Empty;
        IEnumerable<Command> _lastAnswer = new Command[0];
    }

    class CommandControllerEvents : ICommandController
    {
        public void AddCommand(Command c)
        {
            if ((c != null) && (CommandAdded != null))
                CommandAdded(c);
        }

        public void RemoveCommand(Command c)
        {
            if ((c != null) && (CommandRemoved != null))
                CommandRemoved(c);
        }

        public event Action<Command> CommandAdded;
        public event Action<Command> CommandRemoved;
    }

    class Indexer
    {
        public void AddCommand(Command cmd)
        {
            string name = cmd.Name.ToLower();
            _lower[cmd.Name] = name;

            foreach (char c in name)
            {
                HashSet<Command> letter;
                if (!_entries.TryGetValue(c, out letter))
                {
                    letter = new HashSet<Command>();
                    _entries[c] = letter;
                }

                letter.Add(cmd);
            }
        }

        public void RemoveCommand(Command cmd)
        {
            string name = cmd.Name.ToLower();

            foreach (char c in name)
            {
                HashSet<Command> letter;
                if (_entries.TryGetValue(c, out letter))
                {
                    letter.Remove(cmd);
                }
            }
        }

        public IEnumerable<Command> GetMatches(string search)
        {
            if (search.Length == 0)
                return new Command[0];

            search = search.ToLower();

            char c = search[0];

            HashSet<Command> firstCandidates = new HashSet<Command>();
            if (!_entries.TryGetValue(c, out firstCandidates))
            {
                return new Command[0];
            }

            HashSet<Command> candidates = new HashSet<Command>(firstCandidates);

            for (int i = 1; i < search.Length; ++i)
            {
                c = search[i];

                HashSet<Command> letter;
                if (_entries.TryGetValue(c, out letter))
                {
                    candidates.IntersectWith(letter);
                }

                if (candidates.Count == 0)
                {
                    return new List<Command>();
                }
            }

            return candidates.Where(cmd => Matches(_lower[cmd.Name], search)).ToList();
        }

        bool Matches(string name, string search)
        {
            int searchPos = 0;

            for (int namePos = 0; namePos < name.Length; ++namePos)
            {
                if (name[namePos] == search[searchPos])
                {
                    if (++searchPos == search.Length)
                        return true;
                }
            }

            return false;
        }

        Dictionary<string, string> _lower = new Dictionary<string, string>();
        Dictionary<char, HashSet<Command>> _entries = new Dictionary<char, HashSet<Command>>();
    }
}
