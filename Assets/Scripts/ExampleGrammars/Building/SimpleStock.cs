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
        public float stockContinueChance = 0.2f;
        public float billBoardSpawnChance = 0.0f;
        public int billBoardMinHeight = 1;
        public int MinHeight = 3;
        //simple prevention of overlaping billboards
        bool billBoardSpawed;

        // shape parameters:
        public int Width;
        public int Depth;
        

        [SerializeField] LodObject[] wallStyle;
        [SerializeField] LodObject[] groundStyle;
        [SerializeField] LodObject[] roofStyle;
        [SerializeField] LodObject _groundDoor;
        [SerializeField] BillboardStock[] billboards;

        int _currentHeight;

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
                Vector3 localPosition = GetLocalPosition(i);
                
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
            RandomBillboard();

            float randomValue = RandomFloat();
            if (_currentHeight < MinHeight || randomValue < stockContinueChance)
            {

                SimpleStock nextStock = CreateSymbol<SimpleStock>("stock", new Vector3(0, 1, 0));
                nextStock._currentHeight = _currentHeight + 1;
                nextStock.MinHeight = MinHeight;
                nextStock.stockContinueChance = stockContinueChance;
                nextStock.billBoardSpawnChance = billBoardSpawnChance;
                nextStock.billBoardMinHeight = billBoardMinHeight;
                nextStock.billboards = billboards;
                nextStock.billBoardSpawed = billBoardSpawed;

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

        void RandomBillboard()
        {
            if (!billBoardSpawed && _currentHeight >= billBoardMinHeight && RandomFloat() < billBoardSpawnChance)
            {
                BillboardStock nextStock = SpawnPrefab(
                    SelectRandom(billboards));
                int randomSide = RandomInt(4);
                nextStock.Initialize(randomSide, Width, Depth, _currentHeight - 1);
                nextStock.Generate();
                billBoardSpawed = true;
            }
        }

        Vector3 GetLocalPosition(int i)
        {
            return i switch
            {
                0 => new Vector3(-(Width - 1) * 0.5f, 0, 0),// left
                1 => new Vector3(0, 0, (Depth - 1) * 0.5f),// back
                2 => new Vector3((Width - 1) * 0.5f, 0, 0),// right
                3 => new Vector3(0, 0, -(Depth - 1) * 0.5f),// front
                _ => Vector3.zero,
            };
        }
    }
}
