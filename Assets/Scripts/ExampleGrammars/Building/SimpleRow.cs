using UnityEngine;

namespace Demo {
	//this generates a floor
	public class SimpleRow : Shape {
		int Number;
        LodObject[] prefabs=null;
		Vector3 direction;

		public void Initialize(int Number, LodObject[] prefabs, Vector3 dir=new Vector3()) {
			this.Number=Number;
			this.prefabs=prefabs;
			if (dir.magnitude!=0) {
				direction=dir;
			} else {
				direction=new Vector3(0, 0, 1);
			}
		}

		protected override void Execute() {
			if (Number<=0)
				return;
            bool tooFar = WorldSpawner.LOD_Enabled && WorldSpawner.TooFar(transform.position);

            for (int i=0;i<Number;i++) {	// spawn the prefabs, randomly chosen
				int index = RandomInt(prefabs.Length); // choose a random prefab index
				LodObject obj = prefabs[index];
				SpawnLOD( obj, tooFar,
					direction * (i - (Number-1)/2f), // position offset from center
					Quaternion.identity			// no rotation
				);
				

            }
		}
	}
}
