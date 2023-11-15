using UnityEngine;
using System.Collections.Generic;

public sealed class MonopolyBoard : MonoBehaviour
{
    private static MonopolyBoard instance;

    public List<MonopolyNode> Nodes { get; private set; }

    public static MonopolyBoard Instance { get => instance; }

    [SerializeField] public MonopolyNode StartingNode;

    [SerializeField] private List<MonopolySet> monopolySets = new List<MonopolySet>();

    private void Awake()
    {
        instance = this;
        this.Nodes = new List<MonopolyNode>();

        foreach (Transform node in this.transform.GetComponentInChildren<Transform>())
            this.Nodes.Add(node.GetComponent<MonopolyNode>());

        for (int i = 0; i < this.monopolySets.Count; ++i)
        {
            for (int j = 0; j < this.monopolySets[i].NodesInSet.Count; ++j)
                this.monopolySets[i].NodesInSet[j].ImageMonopolySetType.color = this.monopolySets[i].ColorOfSet;
        }
    }



    //public (List<MonopolyCell> list, bool AlSame) PlayerHasAllNodesOfSet(MonopolyCell cell)
    //{
    //    bool alSame = false;

    //    foreach (var nodeSet in nodeSetList)
    //    {
    //        if (nodeSet.nodesInSet.Contains(cell))
    //        {
    //            alSame = nodeSet.nodesInSet.All(node => node.Owner == cell.Owner);
    //            return (nodeSet.nodesInSet, alSame);
    //        }
    //    }

    //    return (null, false);
    //}
}
