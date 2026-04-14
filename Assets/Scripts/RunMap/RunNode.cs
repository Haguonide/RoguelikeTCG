using System.Collections.Generic;

namespace RoguelikeTCG.RunMap
{
    public class RunNode
    {
        public int row;
        public int col;
        public NodeType type;
        public NodeState state;
        public List<RunNode> children = new List<RunNode>();
        public List<RunNode> parents  = new List<RunNode>();

        public RunNode(int row, int col, NodeType type)
        {
            this.row   = row;
            this.col   = col;
            this.type  = type;
            this.state = NodeState.Locked;
        }
    }
}
