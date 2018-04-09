using Devil.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayTask : BTTaskBase
{

    Text mText;
    Color mStartColor;
	public DisplayTask(int id) : base(id) { }

    public override void OnAbort(BehaviourTreeRunner btree)
    {
    }

    public override void OnInitData(BehaviourTreeRunner btree, string jsonData)
    {
        mText = GameObject.Find("FoundYouText").GetComponent<Text>();
        mText.gameObject.SetActive(false);
        mStartColor = mText.color;
    }

    public override EBTTaskState OnTaskStart(BehaviourTreeRunner btree)
    {
        mText.gameObject.SetActive(true);
        return EBTTaskState.running;
    }

    public override EBTTaskState OnTaskTick(BehaviourTreeRunner btree, float deltaTime)
    {
        Color color = Color.Lerp(mStartColor, Color.red, Mathf.PingPong(btree.TaskTime * 2f, 1));
        mText.color = color;
        if (btree.TaskTime > 2)
        {
            mText.gameObject.SetActive(false);
            return EBTTaskState.success;
        }
        else
        {
            return EBTTaskState.running;
        }
    }

}
