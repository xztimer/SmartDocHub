namespace SmartDocHub.Service.Common;

/// <summary>
/// 树形结构（部门、分类）的父级变更校验。
/// 只校验 ParentId != Id 不够：把 A 的父节点设为 A 的孙节点同样会成环，
/// 之后祖先回溯、子树遍历、删除校验全部失效。
/// </summary>
public static class HierarchyGuard
{
    private const int MaxDepth = 1000;

    /// <summary>
    /// 判断把 selfId 挂到 candidateParentId 下是否会成环。
    /// </summary>
    /// <param name="parentOf">整棵树的 Id → ParentId 映射（根节点 ParentId 用 0 表示）</param>
    /// <param name="candidateParentId">拟设置的父节点 Id，0 表示提升为根节点</param>
    /// <param name="selfId">被移动的节点 Id</param>
    public static bool WouldCreateCycle(
        IReadOnlyDictionary<long, long> parentOf,
        long candidateParentId,
        long selfId)
    {
        if (candidateParentId <= 0)
        {
            return false;
        }

        if (candidateParentId == selfId)
        {
            return true;
        }

        var current = candidateParentId;
        var depth = 0;

        while (current > 0 && depth++ < MaxDepth)
        {
            if (current == selfId)
            {
                return true;
            }

            if (!parentOf.TryGetValue(current, out var parent))
            {
                // 父节点不存在（脏数据或已被删除），交由调用方的前置校验处理
                return false;
            }

            if (parent == current)
            {
                return true;
            }

            current = parent;
        }

        // 超过深度上限，视为异常数据，拒绝操作
        return current > 0;
    }
}
