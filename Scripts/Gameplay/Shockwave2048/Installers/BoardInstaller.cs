using Gameplay.Shockwave2048.Board;
using Gameplay.Shockwave2048.Board.ActionStates;
using Gameplay.Shockwave2048.Elements;
using Gameplay.Shockwave2048.Slot;
using PT.Logic.Configs;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Gameplay.Shockwave2048.Installers
{
    public class BoardInstaller : MonoInstaller
    {
        [Space]
        [SerializeField] private GridSlot gridSlotPrefab;
        [SerializeField] private Element elementPrefab;
        [Space]
        [SerializeField] private GridLayoutGroup gridSlotsParent;
        [SerializeField] private Transform elementsParent;
        [Space]
        [SerializeField] private int elementPoolCapacity = 30;
        
        public override void InstallBindings()
        {
            Container.Bind<BoardState>().AsSingle();
            
            Container.Bind<BoardElementPoolService>()
                .FromInstance(new BoardElementPoolService(elementPrefab, elementsParent, elementPoolCapacity))
                .AsSingle();

            Container.BindInterfacesAndSelfTo<BoardGridBuilder>().AsSingle()
                .WithArguments(gridSlotPrefab, gridSlotsParent);

            Container.BindInterfacesAndSelfTo<BoardPlacementController>().AsSingle();
            Container.BindInterfacesAndSelfTo<BoardMoveMergeController>().AsSingle();
            Container.BindInterfacesAndSelfTo<BoardShockwaveController>().AsSingle();
            Container.BindInterfacesAndSelfTo<BoardGridController>().AsSingle();
            Container.BindInterfacesAndSelfTo<BoardTurnsController>().AsSingle();
            Container.BindInterfacesAndSelfTo<BoardBonusesController>().AsSingle();
            Container.BindInterfacesAndSelfTo<BoardProgressionController>().AsSingle();
            Container.BindInterfacesAndSelfTo<BoardActionController>().AsSingle();
            
            if (Container.Resolve<GameConfig>().HasExtraElements) Container.BindInterfacesAndSelfTo<BoardExtraElementSpawner>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<TapActionState>().AsSingle();
            Container.BindInterfacesAndSelfTo<DestroyActionState>().AsSingle();
            Container.BindInterfacesAndSelfTo<SwapActionState>().AsSingle();
            
            Container.Bind<BoardManager>().FromComponentInHierarchy().AsSingle();
            Container.Bind<BoardView>().FromComponentInHierarchy().AsSingle();
            Container.Bind<BoardProgressionView>().FromComponentInHierarchy().AsSingle();
        }
        
    }
}