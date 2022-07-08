namespace BTree.Runtime
{
  public interface IB3
  {
    Node.State Evaluate<T>(T context) where T : IContext;
  }
  
}