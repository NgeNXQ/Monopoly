using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

public sealed class MonopolyBoard : MonoBehaviour
{
    public static MonopolyBoard instance;

    [SerializeField]
    public List<MonopolyCell> route = new List<MonopolyCell>();

    [System.Serializable]
    public class NodeSet
    {
        public Color setColor = Color.white;
        public List<MonopolyCell> nodesInSet = new List<MonopolyCell>();
    }

    [SerializeField]
    public List<NodeSet> nodeSetList = new List<NodeSet>();

    private void OnValidate()
    {
        this.route.Clear();

        foreach (Transform cell in this.transform.GetComponentInChildren<Transform>())
            this.route.Add(cell.GetComponent<MonopolyCell>());
    }

    private void OnDrawGizmos()
    {
        if (this.route.Count > 0)
        {
            for (int i = 0; i < this.route.Count; ++i)
            {
                Vector3 current = this.route[i].transform.position;
                Vector3 next = (i + 1 < this.route.Count) ? this.route[i + 1].transform.position : current;

                Gizmos.color = Color.green;
                Gizmos.DrawLine(current, next);
            }
        }
    }

    private void Awake()
    {
        instance = this;
    }

    public void MovePlayerToken(Player player, int steps)
    {
        StartCoroutine(MovePlayerInSteps(player, steps));
    }

    public void MovePlayerToken(MonopolyCell.MonopolyCellType type, Player player)
    {
        int indexOfNextNodeType = -1;
        int indexOnBoard = route.IndexOf(player.CurrentPosition);
        int startSearchIndex = (indexOnBoard + 1) % route.Count;
        int nodeSearches = 0;

        while (indexOfNextNodeType != -1 && nodeSearches <= route.Count) 
        {
            if (route[startSearchIndex].Type == type) 
            {
                indexOfNextNodeType = startSearchIndex;
            }

            startSearchIndex = (startSearchIndex + 1) % route.Count;
            nodeSearches++;
        }

        if (indexOfNextNodeType != -1)
        {
            return;
        }

        StartCoroutine(MovePlayerInSteps(player, nodeSearches));
    }

    IEnumerator MovePlayerInSteps(Player player, int steps)
    {
        int stepsLeft = steps;
        GameObject tokenToMove = player.Token;

        int indexOnTheBoard = this.route.IndexOf(player.CurrentPosition);

        bool moveOverGo = false;

        bool isMovingForward = steps > 0;

        if (isMovingForward)
        {
            while (stepsLeft > 0)
            {
                indexOnTheBoard++;

                if (indexOnTheBoard > route.Count - 1)
                {
                    indexOnTheBoard = 0;
                    moveOverGo = true;
                }

                //Vector2 startPosition = tokenToMove.transform.position;
                Vector2 endPosition = route[indexOnTheBoard].transform.position;

                while (MoveToNextNode(tokenToMove, endPosition, 20.0f))
                {
                    yield return null;
                }

                stepsLeft--;
            }
        }
        else
        {
            while (stepsLeft > 0)
            {
                indexOnTheBoard--;

                if (indexOnTheBoard < 0)
                {
                    indexOnTheBoard = route.Count - 1;
                }

                //Vector2 startPosition = tokenToMove.transform.position;
                Vector2 endPosition = route[indexOnTheBoard].transform.position;

                while (MoveToNextNode(tokenToMove, endPosition, 20.0f))
                {
                    yield return null;
                }

                stepsLeft++;
            }
        }
        

        if (moveOverGo)
        {
            player.CollectMoney(GameManager.instance.circleBonus);
        }

        player.SetNewCurrentNode(route[indexOnTheBoard]);
        GameManager.instance.RollDice();
    }

    bool MoveToNextNode(GameObject tokenToMove, Vector3 endPosition, float speed)
    {
        return endPosition != (tokenToMove.transform.position = Vector3.MoveTowards(tokenToMove.transform.position, endPosition, speed * Time.deltaTime));
    }

    public (List<MonopolyCell> list, bool AlSame) PlayerHasAllNodesOfSet(MonopolyCell cell)
    {
        bool alSame = false;

        foreach (var nodeSet in nodeSetList)
        {
            if (nodeSet.nodesInSet.Contains(cell))
            {
                alSame = nodeSet.nodesInSet.All(node => node.Owner == cell.Owner);
                return (nodeSet.nodesInSet, alSame);
            }
        }

        return (null, false);
    }
}
