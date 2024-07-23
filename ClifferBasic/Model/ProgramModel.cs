using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;

using ClifferBasic.Services;

namespace ClifferBasic.Model;
internal class ProgramModel {
    private SortedDictionary<int, string[]> _listing = new ();
    private SortedDictionary<int, string[]>.Enumerator _ip;
    private CommandSplitter _splitter;

    internal string[] Listing { 
        get {
            var lines = new List<string> ();
            var lineNumbers = _listing.Keys.OrderBy(x => x).ToList();

            foreach (var lineNumber in lineNumbers) {
                lines.Add(GetLine(lineNumber));
            }

            return lines.ToArray();
        }
        set {
            foreach (var line in value) {
                var tokens = _splitter.Split(line).ToArray();
                if (tokens.Length > 1 && int.TryParse(tokens[0], out int lineNumber)) {
                    SetLine(lineNumber, tokens.Skip(1).ToArray());
                }
            }
        }
    }

    public ProgramModel(CommandSplitter splitter) { 
        _splitter = splitter;
        _ip = _listing.GetEnumerator();
    }

    internal void SetLine(int lineNumber, string[] items) {
        if (_listing.ContainsKey(lineNumber)) {
            _listing[lineNumber] = items;
        }
        else {
            _listing.Add(lineNumber, items);
        }
    }

    internal string GetLine(int lineNumber) {
        string line = string.Join(" ", _listing[lineNumber]);
        return $"{lineNumber} {line}";
    }

    internal bool HasLine(int lineNumber) => _listing.ContainsKey(lineNumber);

    internal void New() { 
        _listing.Clear();
    }

    internal void Reset() {
        _ip = _listing.GetEnumerator();
    }

    internal bool IsAtEnd { get; private set; } = false;

    internal string[] Current {
        get {
            return _ip.Current.Value;
        }
    }

    internal string[]? Advance() {
        if (_ip.MoveNext()) {
            return _ip.Current.Value;
        }

        IsAtEnd = true;
        return null;
    }

    internal void Goto(int lineNumber) {
    }
}
