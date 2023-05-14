using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ListSorted
{
    internal class Program
    {
        private static ListRand CreateListRand(int itemCount = 5)
        {
            var listRand = new ListRand();
            var items = new List<ListNode>();
            for (var i = 0; i < itemCount; i++)
            {
                var node = new ListNode();
                if (i == 0)
                {
                    node.Prev = null;
                }
                else
                {
                    var prevNode = items[i - 1];
                    node.Prev = prevNode;
                    prevNode.Next = node;
                }

                node.Data = $"data_{i}";
                items.Add(node);
            }

            items[1].Rand = items[1];
            items[2].Rand = items[4];
            items[3].Rand = items[0];

            listRand.Head = items[0];
            listRand.Tail = items[itemCount - 1];
            listRand.Count = itemCount;
            return listRand;
        }

        public static void Main(string[] args)
        {
            var listRand = CreateListRand();
            var path = "./listRand.txt";
            using (var fileStream = new FileStream(path, FileMode.OpenOrCreate))
            {
                listRand.Serialize(fileStream);

                fileStream.Seek(0, SeekOrigin.Begin);
                var listFromFile = new ListRand();
                listFromFile.Deserialize(fileStream);

                var isIntegrityIntact = CheckIntegrity(listRand, listFromFile, out var reason);
                Console.WriteLine(isIntegrityIntact ? "List integrity is intact" : $"List integrity failed: {reason}");
            }

            bool CheckIntegrity(ListRand original, ListRand other, out string reason)
            {
                reason = "";
                
                if (original.Count != other.Count)
                {
                    reason = $"List item counts do not match, original: {original.Count}, new {other.Count}";
                    return false;
                }

                if (other.Head == null)
                {
                    reason = "Head is null";
                    return false;
                }

                if (other.Tail == null)
                {
                    reason = "Tail is null";
                    return false;
                }

                var currentNode = other.Head;
                var originalCurrentNode = original.Head;
                var index = 0;
                while (currentNode != null)
                {
                    if (currentNode.Data != originalCurrentNode.Data)
                    {
                        reason = $"Data strings do not match, original: {originalCurrentNode.Data}, new: {currentNode.Data}, node index: {index}";
                        return false;
                    }
                    
                    currentNode = currentNode.Next;
                    originalCurrentNode = originalCurrentNode.Next;
                    index++;
                }

                return true;
            }
        }

        private class ListNode
        {
            public string Data;
            public ListNode Next;
            public ListNode Prev;
            public ListNode Rand; // произвольный элемент внутри списка 
        }


        private class ListRand
        {
            public int Count;
            public ListNode Head;
            public ListNode Tail;

            public void Serialize(FileStream s)
            {
                void TryWrite(byte[] data)
                {
                    if (!s.CanWrite)
                    {
                        throw new Exception("Could not write data to fileStream: stream is not available for writing");
                    }
                    
                    s.Write(data, 0, data.Length);
                }

                if (s.Position != 0)
                {
                    s.Seek(0, SeekOrigin.Begin);
                }
                
                //Assign ids to each of the nodes in the list:
                var id = 0;
                var nodeIds = new Dictionary<ListNode, int>();
                var currentNode = Head;
                while (currentNode != null)
                {
                    nodeIds.Add(currentNode, id);
                    id++;
                    currentNode = currentNode.Next;
                }
                
                TryWrite(BitConverter.GetBytes(Count));
                currentNode = Head;
                while (currentNode != null)
                {
                    var dataBytes = Encoding.UTF8.GetBytes(currentNode.Data);
                    TryWrite(BitConverter.GetBytes(dataBytes.Length));
                    TryWrite(dataBytes);
                    var linkedIndex = currentNode.Rand != null ? nodeIds[currentNode.Rand] : -1;
                    TryWrite(BitConverter.GetBytes(linkedIndex));
                    
                    currentNode = currentNode.Next;
                }
            }

            public void Deserialize(FileStream s)
            {
                byte[] TryRead(int byteCount)
                {
                    if (!s.CanRead)
                    {
                        throw new Exception("Could not read from file stream: file is not available for reading");
                    }

                    var buffer = new byte[byteCount];
                    s.Read(buffer, 0, byteCount);
                    return buffer;
                }
                
                if (s.Position != 0)
                {
                    s.Seek(0, SeekOrigin.Begin);
                }
                
                //Read Item Count from stream
                var count = BitConverter.ToInt32(TryRead(4), 0);
                Count = count;

                //Read Data and random links from stream
                //Also create nodes 
                var nodes = new Dictionary<int, ListNode>();
                var linkedNodes = new Dictionary<int, int>();
                for (var i = 0; i < count; i++)
                {
                    var dataBytesCount = BitConverter.ToInt32(TryRead(4), 0);
                    var data = Encoding.UTF8.GetString(TryRead(dataBytesCount));
                    var linkedNode = -1;
                    BitConverter.ToInt32(TryRead(4), 0);

                    var node = new ListNode
                    {
                        Data = data
                    };

                    nodes[i] = node;
                    linkedNodes[i] = linkedNode;
                }

                // Create links between nodes
                Head = nodes[0];
                Tail = nodes[count - 1];
                for (var i = 0; i < count; i++)
                {
                    var node = nodes[i];
                    var linkedNode = linkedNodes[i];

                    if (i > 0)
                    {
                        node.Prev = nodes[i - 1];
                    }
                    
                    if (i < count - 1)
                    {
                        node.Next = nodes[i + 1];
                    }
                    if (linkedNode >= 0)
                    {
                        node.Rand = nodes[linkedNode];
                    }
                }
            }
        }
    }
}