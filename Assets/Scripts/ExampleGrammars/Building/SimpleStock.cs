using UnityEngine;

namespace Demo
{
    [System.Serializable]
    public class LodObject
    {
        public GameObject High;
        public GameObject Low;
        public Material HighMat;
        public Material LowMat;
    }
    public class SimpleStock : Shape
    {
        // grammar rule probabilities:
        public float stockContinueChance = 0.2f;
        public float billBoardSpawnChance = 0.0f;
        public float IndentSpawnChance = .4f;
        public int billBoardMinHeight = 1;
        public int MinHeight = 3;
        //simple prevention of overlaping billboards
        int _lastBillBoardSpawed = 0;

        // shape parameters:
        public int Width;
        public int Depth;
        

        [SerializeField] LodObject[] wallStyle;
        [SerializeField] LodObject[] groundStyle;
        [SerializeField] LodObject[] roofStyle;
        [SerializeField] LodObject _groundDoor;
        [SerializeField] BlockRow _indent;
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
            float randomValue = RandomFloat();
            bool willSpawnBody = _currentHeight < MinHeight || randomValue < stockContinueChance;
            if (_currentHeight == 0)
                SpawnGround();
            else if (willSpawnBody && _currentHeight > 1 && RandomFloat() < IndentSpawnChance)
                SpawnSegment();
            else
                SpawnBody();

            RandomBillboard();

            if (willSpawnBody)
            {

                SimpleStock nextStock = CreateSymbol<SimpleStock>("stock", new Vector3(0, 1, 0));
                nextStock._currentHeight = _currentHeight + 1;
                nextStock.MinHeight = MinHeight;
                nextStock.stockContinueChance = stockContinueChance;
                nextStock.billBoardSpawnChance = billBoardSpawnChance;
                nextStock.billBoardMinHeight = billBoardMinHeight;
                nextStock.billboards = billboards;
                nextStock._lastBillBoardSpawed = _lastBillBoardSpawed;
                nextStock._indent = _indent;
                nextStock.IndentSpawnChance = IndentSpawnChance;

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

        void SpawnGround()
        {
            int doorSideIndex = RandomInt(4);
            for (int i = 0; i < 4; i++)
            {
                Vector3 localPosition = GetLocalPosition(i);
                LodObject door = doorSideIndex == i ? _groundDoor : null;
                GroundRow ground = CreateSymbol<GroundRow>("wall", localPosition, Quaternion.Euler(0, i * 90, 0));
                ground.Initialize(i % 2 == 1 ? Width : Depth, groundStyle, door);
                ground.Generate();
            }
        }

        void SpawnBody()
        {
            for (int i = 0; i < 4; i++)
            {
                Vector3 localPosition = GetLocalPosition(i);
                SimpleRow newRow = CreateSymbol<SimpleRow>("wall", localPosition, Quaternion.Euler(0, i * 90, 0));
                newRow.Initialize(i % 2 == 1 ? Width : Depth, wallStyle);
                newRow.Generate();
            }
        }

        void SpawnSegment()
        {
            _currentHeight--;

            BlockRow newRow = SpawnPrefab(_indent);
            newRow.Initialize(Width, Depth);
            newRow.Generate();
            newRow.transform.localPosition = Vector3.up * (1 - newRow.Height * .5f);
            //move this instance down, since the indent is smaller than 1 unit
            transform.localPosition =  Vector3.up * newRow.Height; 
        }

        void RandomBillboard()
        {
            if (_currentHeight - _lastBillBoardSpawed >= billBoardMinHeight && RandomFloat() < billBoardSpawnChance)
            {
                BillboardStock nextStock = SpawnPrefab(
                    SelectRandom(billboards));
                int randomSide = RandomInt(4);
                nextStock.Initialize(randomSide, Width, Depth, _currentHeight - _lastBillBoardSpawed - 1);
                nextStock.Generate();
                _lastBillBoardSpawed = _currentHeight;
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
