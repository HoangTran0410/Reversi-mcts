using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Reversi_mcts.Utils
{
    public static class MemoryUtils
    {
        // https://stackoverflow.com/a/31548017/11898496
        public static int GetObjectSize(object testObject)
        {
            var bf = new BinaryFormatter();
            var ms = new MemoryStream();
            bf.Serialize(ms, testObject);
            var array = ms.ToArray();
            return array.Length;
        }
    }
}