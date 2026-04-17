using Gameplay.Shockwave2048.Elements;
using UnityEngine;
using UnityEngine.Pool;

namespace Gameplay.Shockwave2048.Board
{
    public class BoardElementPoolService
    {
        public ObjectPool<Element> Pool { get; private set; }

        public BoardElementPoolService(Element prefab, Transform parent, int capacity)
        {
            Pool = new ObjectPool<Element>(
                () => GameObject.Instantiate(prefab, parent),
                e => e.gameObject.SetActive(true),
                e => e.gameObject.SetActive(false),
                defaultCapacity: capacity
            );
        }
    }
}