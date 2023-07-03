using UnityEngine;
using Zenject;

public class LocationInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<TextAppear>();
    }
}