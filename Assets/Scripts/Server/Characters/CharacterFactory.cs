using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Society.Characters
{
    public class CharacterFactory
    {
        public static List<Character> CreateCharacters(Society.Server.Server server, int count)
        {
            return Enumerable.Range(1, count).Select(i => new Character(server)).ToList();
        }
    }
}
