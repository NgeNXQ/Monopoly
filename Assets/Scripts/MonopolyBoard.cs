using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public sealed class MonopolyBoard : MonoBehaviour
{
    [SerializeField]
    public List<MonopolyCell> route;

    private void Awake() => this.route = new List<MonopolyCell>();

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
}
