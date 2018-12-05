using UnityEngine;

namespace Devil.GamePlay
{
    public interface IGamePlayer : INamed
    {
        Vector3 position { get; set; }
        Vector3 velocity { get; set; }
        Quaternion rotation { get; set; }

        bool isGrounded { get; }
        bool isStoped { get; }
        bool isAlive { get; }
        Vector3 forward { get; }
        Vector3 up { get; }
        Vector3 right { get; }

        Animator AttachedAnimator { get; }
        CharacterController AttachedController { get; }
        Rigidbody AttachedRigidbody { get; }
        T GetComponent<T>();

        IPlayerMotion CurrentBaseMotion { get; }
        IPlayerMotion CurrentAdditiveMotion { get; }
        IPlayerMotion FindMotion(int motionId);
        IPlayerMotion FindMotion(string motionName);

        void AddInput(int flag, object data);
        
    }
}