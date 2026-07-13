using System;
using UnityEngine;

public class PuzzleEventManager : MonoBehaviour
{
    public static PuzzleEventManager Instance { get; private set; }

    // 已有的拾取事件
    public event Action<string> OnItemGrabbed;

    // 新增放置事件：参数为 (物品ID, 落点物体的GameObject)
    public event Action<string, GameObject> OnItemDropped;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void NotifyItemGrabbed(string objectId)
    {
        OnItemGrabbed?.Invoke(objectId);
    }

    public void NotifyItemDropped(string objectId, GameObject surface)
    {
        OnItemDropped?.Invoke(objectId, surface);
    }
}