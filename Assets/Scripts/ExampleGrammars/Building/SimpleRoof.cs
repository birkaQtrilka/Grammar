using UnityEngine;

namespace Demo {
	public class SimpleRoof : Shape {

		// shape parameters:
		int Width;
		int Depth;

		LodObject[] roofStyle;

		// (offset) values for the next layer:
		int newWidth;
		int newDepth;
		Antena _antena;


		public void Initialize(int Width, int Depth, LodObject[] roofStyle, Antena antena) {
			this.Width=Width;
			this.Depth=Depth;
			this.roofStyle=roofStyle;
			_antena = antena;
		}


		protected override void Execute() {
			if (Width==0 || Depth==0)
				return;

			newWidth=Width;
			newDepth=Depth;

			CreateFlatRoofPart();
			
            CreateNextPart();
        }

		void CreateFlatRoofPart() {
			// Randomly create two roof strips in depth direction or in width direction:
			int side = RandomInt(2);
			SimpleRow flatRoof;

			switch (side) {
				// Add two roof strips in depth direction
				case 0:
					for (int i = 0; i<2; i++) {
						flatRoof = CreateSymbol<SimpleRow>("roofStrip",new Vector3((Width-1)*(i-0.5f), 0, 0));
						flatRoof.Initialize(Depth, roofStyle);
						flatRoof.Generate();
					}
					newWidth-=2;
				break;
				// Add two roof strips in width direction
				case 1:
					for (int i = 0; i<2; i++) {
						flatRoof = CreateSymbol<SimpleRow>("roofStrip",new Vector3(0,0,(Depth-1)*(i-0.5f)));
						flatRoof.Initialize(Width, roofStyle,new Vector3(1,0,0));
						flatRoof.Generate();
					}
					newDepth-=2;
				break;
			}
		}

		void CreateNextPart() {
			// randomly continue with a roof or a stock:
			if (newWidth<=0 || newDepth<=0)
			{
				if (_antena == null) return;
				int minSide = Mathf.Min(Width, Depth);
				float startSize = RandomFloat(.2f, .8f);
				float halfMinSide = minSide * startSize;
				float randomX = RandomFloat(-halfMinSide, halfMinSide);
				float randomZ = RandomFloat(-halfMinSide, halfMinSide);
				Antena antena = SpawnPrefab(_antena, new Vector3(randomX,0,randomZ));
                antena.Init( startSize, minSide, minSide, true, RandomFloat(.08f, .2f));
				antena.Generate();
				
				return;
			}

			SimpleRoof nextRoof = CreateSymbol<SimpleRoof>("roof");
			nextRoof.Initialize(newWidth, newDepth, roofStyle, _antena);
			nextRoof.Generate(buildDelay);
		}
	}
}