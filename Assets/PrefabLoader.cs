using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Entities.Serialization;
using UnityEngine;
using Unity.Scenes;
using Unity.Transforms;

public class PrefabLoader : MonoBehaviour
{
    public GameObject prefab;
    
    public class Baker: Baker<PrefabLoader>
    {
        public override void Bake(PrefabLoader authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<PrefabLoadComponent>(entity, new PrefabLoadComponent
            {
                prefab = new EntityPrefabReference(authoring.prefab),
            });
        }
    }
}

public struct PrefabLoadComponent: IComponentData
{
    public EntityPrefabReference prefab;
}

public partial class PrefabLoadSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.WithNone<RequestEntityPrefabLoaded, PrefabLoadResult>().WithImmediatePlayback()
            .ForEach((Entity entity, EntityCommandBuffer command_buffer, PrefabLoadComponent load_component) =>
        {
            command_buffer.AddComponent<RequestEntityPrefabLoaded>(entity, new RequestEntityPrefabLoaded
            {
                Prefab = load_component.prefab,
            });
        }).Run();
        Entities.WithImmediatePlayback().WithAll<PrefabLoadComponent>()
            .ForEach((Entity entity, EntityCommandBuffer command_buffer, LocalTransform transform, PrefabLoadResult load_result) =>
        {
            Entity instance = command_buffer.Instantiate(load_result.PrefabRoot);
            command_buffer.SetComponent<LocalTransform>(instance, transform);
            command_buffer.DestroyEntity(entity);
        }).Run();
    }
}
