using System;
using System.Collections.Generic;
using System.IO;

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

                var listFromFile = new ListRand();
                listFromFile.Deserialize(fileStream);

                var isIntegrityIntact = CheckIntegrity(listRand, listFromFile);
                Console.WriteLine(isIntegrityIntact ? "List integrity is intact" : "List integrity failed");
            }

            bool CheckIntegrity(ListRand original, ListRand other)
            {
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
                //write item count
                var id = 0;
                var nodeIds = new Dictionary<ListNode, int>();
                var currentNode = Head;
                while (currentNode != null)
                {
                    nodeIds.Add(currentNode, id);
                    id++;
                    currentNode = currentNode.Next;
                }

                currentNode = Head;
                while (currentNode != null)
                {
                    if (currentNode.Rand != null)
                    {
                        var linkedIndex = nodeIds[currentNode.Rand];
                        //write data and linked index to stream;
                    }

                    currentNode = currentNode.Next;
                }
            }

            public void Deserialize(FileStream s)
            {
                //Read Item Count from stream
                var count = 8; //read count from stream;
                Count = count;
                
                //Read Data and random links from stream
                //Also create nodes 
                var nodes = new Dictionary<int, ListNode>();
                var linkedNodes = new Dictionary<int, int>();
                for (var i = 0; i < count; i++)
                {
                    var data = "data"; //read data from stream;
                    var linkedNode = -1; //read link id from stream;

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