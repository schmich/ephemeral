using System;
using System.Collections.Generic;
using System.Linq;
using Ephemeral.Commands;
using System.ComponentModel.Composition;

namespace Ephemeral.Commands
{
    public interface ICommandProvider
    {
        IEnumerable<ICommand> GetSuggestions(string input);
    }

    class CommandPriortyComparer : IComparer<ICommand>
    {
        public CommandPriortyComparer(string input)
        {
            _input = input;
        }

        public int Compare(ICommand p, ICommand q)
        {
            return GetPriority(p) - GetPriority(q);   
        }

        int GetPriority(ICommand c)
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

    [Export(typeof(ICommandStore))]
    [Export(typeof(ICommandProvider))]
    public class PatternCommandProvider : ICommandStore, ICommandProvider
    {
        public void AddCommand(ICommand c)
        {
            _indexer.AddCommand(c);
        }

        public void RemoveCommand(ICommand c)
        {
            _indexer.RemoveCommand(c);
        }

        public IEnumerable<ICommand> GetSuggestions(string input)
        {
            List<ICommand> answer = new List<ICommand>(_indexer.GetMatches(input));
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
        IEnumerable<ICommand> _lastAnswer = new ICommand[0];
    }

    class Indexer
    {
        public void AddCommand(ICommand cmd)
        {
            string name = cmd.Name.ToLower();
            _lower[cmd.Name] = name;

            foreach (char c in name)
            {
                HashSet<ICommand> letter;
                if (!_entries.TryGetValue(c, out letter))
                {
                    letter = new HashSet<ICommand>();
                    _entries[c] = letter;
                }

                letter.Add(cmd);
            }
        }

        public void RemoveCommand(ICommand cmd)
        {
            string name = cmd.Name.ToLower();

            foreach (char c in name)
            {
                HashSet<ICommand> letter;
                if (_entries.TryGetValue(c, out letter))
                {
                    letter.Remove(cmd);
                }
            }
        }

        public IEnumerable<ICommand> GetMatches(string search)
        {
            if (search.Length == 0)
                return new ICommand[0];

            search = search.ToLower();

            char c = search[0];

            HashSet<ICommand> firstCandidates = new HashSet<ICommand>();
            if (!_entries.TryGetValue(c, out firstCandidates))
            {
                return new ICommand[0];
            }

            HashSet<ICommand> candidates = new HashSet<ICommand>(firstCandidates);

            for (int i = 1; i < search.Length; ++i)
            {
                c = search[i];

                HashSet<ICommand> letter;
                if (_entries.TryGetValue(c, out letter))
                {
                    candidates.IntersectWith(letter);
                }

                if (candidates.Count == 0)
                {
                    return new List<ICommand>();
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
        Dictionary<char, HashSet<ICommand>> _entries = new Dictionary<char, HashSet<ICommand>>();
    }
}
