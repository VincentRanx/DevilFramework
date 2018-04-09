using Devil.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[BTSharedType]
public class PlayerController : MonoBehaviour
{
    static List<PlayerController> mPlayers = new List<PlayerController>();
    public static List<PlayerController> AllPlayers { get { return mPlayers; } }

    // 视锥夹角
    public float m_Fov = 60;
    // 视锥宽高比
    public float m_FovAspect = 2f;
    // 视野距离
    public float m_ViewDistance = 50f;
    // 警戒范围
    public float m_AlertRadiaus = 3;

    // 眼睛节点
    public Transform m_EyeTrans;

    public bool m_IsHost;
    public Renderer m_ModelRenderer;
    public bool FoundTarget { get; set; }

    public float m_Speed = 3.5f;
    public float m_AngularSpeed = 270f;

    protected Animator mAnim;

    protected Material mMat;
    float mSpeed;
    Vector3 mDirection;
    Rigidbody mRig;
    bool mMove;

    Vector3 mLookAtPoint;
    float mLookAtWeight;

    private void Awake()
    {
        if (!mPlayers.Contains(this))
        {
            mPlayers.Add(this);
            Debug.Log(string.Format("Add Player: {0}", name), this);
        }
        if (m_ModelRenderer)
            mMat = m_ModelRenderer.material;
        else
            mMat = GetComponent<Renderer>().material;
        mAnim = GetComponent<Animator>();
        mRig = GetComponent<Rigidbody>();
    }

    private void OnDestroy()
    {
        Debug.Log(string.Format("Remove Player: {0}", name), this);
        mPlayers.Remove(this);
    }

    protected virtual void Start()
    {
        mSpeed = 0;
        mDirection = transform.forward;
        mSpeed = 0;
    }

    protected virtual void Update()
    {
        if (m_IsHost && mMat)
        {
            mMat.color = FoundTarget ? Color.red : Color.green;
        }
    }

    private void OnAnimatorMove()
    {

        float forward = Vector3.Dot(mDirection, transform.forward);
        float turn = Vector3.Dot(mDirection, transform.right);
        if (forward < 0)
            turn = turn < 0 ? -1 : 1;
        forward = Mathf.Clamp01(forward);
        Vector3 v = forward * mSpeed * mDirection * m_Speed;
        v.y = mRig.velocity.y;
        mRig.velocity = v;

        if (Vector3.Angle(transform.forward, mDirection) >= 1)
            mRig.MoveRotation(Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(mDirection, Vector3.up), m_AngularSpeed * Time.deltaTime));

        mAnim.SetFloat("Forward", forward * mSpeed);
        mAnim.SetFloat("Turn", turn);
        mAnim.SetBool("OnGround", true);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if(mLookAtWeight > 0.1f)
        {
            mAnim.SetLookAtPosition(mLookAtPoint);
            mAnim.SetLookAtWeight(mLookAtWeight);
            mLookAtWeight -= Time.deltaTime;
        }
    }

    private void LateUpdate()
    {
        if(!mMove)
        {
            mSpeed = Mathf.Lerp(mSpeed, 0, 4 * Time.deltaTime);
        }
        mMove = false;
    }

    public void Move(Vector3 dir)
    {
        mSpeed = Mathf.Clamp01(dir.magnitude);
        dir = Vector3.ProjectOnPlane(dir, Vector3.up).normalized;
        if(dir.sqrMagnitude > 0.01)
            mDirection = dir;
        mMove = true;
    }

    public void LookAt(Vector3 point)
    {
        mLookAtPoint = point;
        mLookAtWeight = 1;
    }

    public bool IsTargetInSight(Transform target)
    {
        if (target == null)
            return false;
        if (Vector3.Distance(target.position, m_EyeTrans.position)<m_AlertRadiaus)
            return true;
        Vector3 p = m_EyeTrans.worldToLocalMatrix.MultiplyPoint(target.position);
        if (p.z < 0 || p.z > m_ViewDistance)
            return false;
        Vector3 pxoz = new Vector3(p.x, 0, p.z);
        float ang = Vector3.Angle(pxoz, Vector3.forward);
        if (ang > m_Fov)
            return false;
        Vector3 pyoz = new Vector3(0, p.y, p.z);
        if (ang > m_Fov / m_FovAspect)
            return false;
        RaycastHit hit;
        if(Physics.Raycast(m_EyeTrans.position, target.position - m_EyeTrans.position, out hit, p.magnitude))
        {
            return hit.transform == target;
        }
        else
        {
            return false;
        }
    }

    private void OnDrawGizmos()
    {
        if (m_IsHost && m_EyeTrans != null)
        {
            Gizmos.color = FoundTarget ? Color.red : Color.green;
            Gizmos.matrix = m_EyeTrans.localToWorldMatrix;
            Gizmos.DrawFrustum(Vector3.zero, m_Fov, m_ViewDistance, 0f, 2);
            Gizmos.DrawWireSphere(Vector3.zero, m_AlertRadiaus);
        }
    }
    
}
