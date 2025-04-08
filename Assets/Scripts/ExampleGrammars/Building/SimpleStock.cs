using UnityEngine;

namespace Demo
{
    [System.Serializable]
    public class LodObject
    {
        public GameObject High;
        public GameObject Low;
    }
    public class SimpleStock : Shape
    {
        // grammar rule probabilities:
        public float stockContinueChance = 0.0f;
        public int MinHeight = 3;

        // shape parameters:
        public int Width;
        public int Depth;
        

        [SerializeField] LodObject[] wallStyle;
        [SerializeField] LodObject[] groundStyle;
        [SerializeField] LodObject[] roofStyle;
        [SerializeField] LodObject _groundDoor;

        float _currentHeight;

        public void Initialize(int Width, int Depth, LodObject[] wallStyle, LodObject[] roofStyle)
        {
            this.Width = Width;
            this.Depth = Depth;
            this.wallStyle = wallStyle;
            this.roofStyle = roofStyle;
        }

        protected override void Execute()
        {
            // Create four walls:
            int doorSideIndex = RandomInt(4);
            for (int i = 0; i < 4; i++)
            {
                Vector3 localPosition = new();
                switch (i)
                {
                    case 0:
                        localPosition = new Vector3(-(Width - 1) * 0.5f, 0, 0); // left
                        break;
                    case 1:
                        localPosition = new Vector3(0, 0, (Depth - 1) * 0.5f); // back
                        break;
                    case 2:
                        localPosition = new Vector3((Width - 1) * 0.5f, 0, 0); // right
                        break;
                    case 3:
                        localPosition = new Vector3(0, 0, -(Depth - 1) * 0.5f); // front
                        break;
                }
                if(_currentHeight == 0)
                {
                    LodObject door = doorSideIndex == i ? _groundDoor : null;
                    GroundRow ground = CreateSymbol<GroundRow>("wall", localPosition, Quaternion.Euler(0, i * 90, 0));
                    ground.Initialize(i % 2 == 1 ? Width : Depth, groundStyle, door);
                    ground.Generate();
                    continue;
                }
                SimpleRow newRow = CreateSymbol<SimpleRow>("wall", localPosition, Quaternion.Euler(0, i * 90, 0));
                newRow.Initialize(i % 2 == 1 ? Width : Depth, wallStyle);
                newRow.Generate();
            }
           
            // Continue with a stock or with a roof (random choice):
            float randomValue = RandomFloat();
            if (_currentHeight < MinHeight || randomValue < stockContinueChance)
            {
                SimpleStock nextStock = CreateSymbol<SimpleStock>("stock", new Vector3(0, 1, 0));
                nextStock._currentHeight = _currentHeight + 1;
                nextStock.MinHeight = MinHeight;
                nextStock.Initialize(Width, Depth, wallStyle, roofStyle);
                nextStock.Generate(buildDelay);
            }
            else
            {
                SimpleRoof nextRoof = CreateSymbol<SimpleRoof>("roof", new Vector3(0, 1, 0));
                nextRoof.Initialize(Width, Depth, roofStyle, wallStyle);
                nextRoof.Generate(buildDelay);
            }
        }
    }
}
