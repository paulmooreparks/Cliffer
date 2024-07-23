using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;

using ClifferBasic.Services;

namespace ClifferBasic.Model;
internal class ProgramModel {
    private Dictionary<int, string[]> _listing = new ();

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
            var splitter = Utility.GetService<CommandSplitter>()!;
            foreach (var line in value) {
                var tokens = splitter.Split(line).ToArray();
                if (tokens.Length > 1 && int.TryParse(tokens[0], out int lineNumber)) {
                    SetLine(lineNumber, tokens.Skip(1).ToArray());
                }
            }
        }
    }
    public ProgramModel() { }

    public ProgramModel(Dictionary<int, string[]> listing) {
        _listing = listing;
    }

    internal void SetLineDictionary(Dictionary<int, string[]> listing) {
        _listing = listing;
    }

    internal Dictionary<int, string[]> GetLineDictionary() => _listing;

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
}
