﻿namespace Umbraco.Core.Migrations
{
    /// <summary>
    /// Marker interface for migration expressions
    /// </summary>
    public interface IMigrationExpression
    {
        string Process(IMigrationContext context); // fixme kill
        void Execute();
    }
}
