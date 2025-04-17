namespace Demo
{
    using UnityEngine;
    public class BlockRow : Shape
    {
        [SerializeField] LodObject[] prefabs = null;
        [SerializeField] float _indentDepth= .07f;

        int _width;
        int _depth;
        public float Height { get; private set; }

        public void Initialize(int width, int depth)
        {
            _width = width;
            _depth = depth;
        }

        protected override void Execute()
        {
            bool tooFar = WorldSpawner.LOD_Enabled && WorldSpawner.TooFar(transform.position);
            LodObject block = prefabs[RandomInt(prefabs.Length)];
            var inst = SpawnLOD(block, tooFar);

            Height = inst.transform.localScale.y;
            inst.transform.localScale = new Vector3(_width - _indentDepth, Height, _depth - _indentDepth);
        }
    }
}
