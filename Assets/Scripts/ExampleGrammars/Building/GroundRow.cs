using UnityEngine;
namespace Demo
{
    //has the addtional rule of spawning a unique item once on that row
    public class GroundRow : Shape
    {
        int Number;
        LodObject[] prefabs = null;
        Vector3 direction;
        LodObject _doorPrefab;

        public void Initialize(int Number, LodObject[] prefabs, LodObject doorPrefab = null, Vector3 dir = new Vector3())
        {
            this.Number = Number;
            this.prefabs = prefabs;
            _doorPrefab = doorPrefab;
            if (dir.magnitude != 0)
            {
                direction = dir;
            }
            else
            {
                direction = new Vector3(0, 0, 1);
            }
        }

        protected override void Execute()
        {
            if (Number <= 0)
                return;
            int doorIndex = RandomInt(Number);
            bool tooFar = WorldSpawner.LOD_Enabled && WorldSpawner.TooFar(transform.position);

            for (int i = 0; i < Number; i++)
            {   // spawn the prefabs, randomly chosen
                LodObject tile = _doorPrefab != null && i == doorIndex ? _doorPrefab : prefabs[RandomInt(prefabs.Length)];
                SpawnLOD(tile, tooFar,
                    direction * (i - (Number - 1) / 2f), // position offset from center
                    Quaternion.identity         // no rotation
                );
            }
        }
    }
}