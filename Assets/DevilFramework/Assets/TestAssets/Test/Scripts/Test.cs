using Devil.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Test : MonoBehaviour {

    public Transform m_Target;
    public bool m_Calculate;
    NavMeshPath mPath;

    NavMeshAgent mAgent;

    private void Start()
    {
        mPath = new NavMeshPath();
        mAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (mAgent.isOnOffMeshLink)
        {
            mAgent.ActivateCurrentOffMeshLink(true);
            //mAgent.CompleteOffMeshLink();
            //transform.position = Vector3.MoveTowards(transform.position, mAgent.currentOffMeshLinkData.endPos, Time.deltaTime);
            //if (Vector3.Distance(transform.position, mAgent.currentOffMeshLinkData.endPos) < 0.01f)
            //{
            //    mAgent.CompleteOffMeshLink();
            //}
        }
    }

    private void OnGUI()
    {
        GUILayout.Label(mPath.status.ToString());
        GUILayout.Label("Is On Mesh " + mAgent.isOnNavMesh);
        GUILayout.Label("Is On Link " + mAgent.isOnOffMeshLink);
    }

    private void OnDrawGizmos()
    {
        if (m_Calculate && m_Target)
        {
            m_Calculate = false;
            NavMesh.CalculatePath(transform.position, m_Target.position, 1, mPath);
            mAgent.SetDestination(m_Target.position);
        }
        if (mAgent)
            mPath = mAgent.path;
        if (mPath != null)
        {
            Vector3[] ps = mPath.corners;
            Gizmos.color = Color.cyan;
            for (int i = 1; i < ps.Length; i++)
            {
                Gizmos.DrawLine(ps[i - 1], ps[i]);
                GizmosUtil.MarkInScene(ps[i], 20, 0);
            }
        }
        if (mAgent)
        {
            Gizmos.color = Color.green;
            GizmosUtil.MarkInScene(mAgent.nextPosition, 30, 0);
        }
    }
}
