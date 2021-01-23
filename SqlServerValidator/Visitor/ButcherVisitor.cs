using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Main.Inclusion.Carved.Result;
using Main.Sql.Identifier;
using Main.Sql.VariableRef;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServerValidator.Identifier;

namespace SqlServerValidator.Visitor
{
    [DebuggerDisplay("{TableNames}")]
    public class ButcherVisitor : TSqlFragmentVisitor, ICarveResult
    {
        /// <summary>
        /// duplicates allowed
        /// </summary>
        private List<ITableName> _tableList = new List<ITableName>();

        /// <summary>
        /// duplicates allowed
        /// </summary>
        private List<IColumnName> _columnList = new List<IColumnName>();

        /// <summary>
        /// duplicates allowed
        /// </summary>
        private List<IIndexName> _indexList = new List<IIndexName>();

        /// <summary>
        /// NO duplicates allowed
        /// </summary>
        private Dictionary<string, IVariableRef2> _variableReferenceList = new Dictionary<string, IVariableRef2>(SqlVariableStringComparer.Instance);

        public IReadOnlyList<ITableName> TableList => _tableList;

        public IReadOnlyList<IColumnName> ColumnList => _columnList;

        public IReadOnlyList<IIndexName> IndexList => _indexList;


        public IReadOnlyList<ITableName> TempTableList => _tableList.FindAll(j => j.IsTempTable);

        public IReadOnlyList<ITableName> TableVariableList => _tableList.FindAll(j => j.IsTableVariable);

        public IReadOnlyCollection<IVariableRef> VariableReferenceList => _variableReferenceList.Values;

        /// <summary>
        /// did * used in select queries?
        /// </summary>
        public bool IsStarReferenced
        {
            get;
            private set;
        }

        public string TableNames
        {
            get
            {
                if (_tableList.Count == 0)
                {
                    return
                        "No table references";
                }

                return
                    string.Join(" , ", _tableList.Select(j => j.FullTableName).Distinct());
            }
        }

        public string ColumnNames
        {
            get
            {
                if (_columnList.Count == 0)
                {
                    return
                        "No column references";
                }

                return
                    string.Join(" , ", _columnList.Select(j => j.ColumnName).Distinct());
            }
        }

        public string IndexNames
        {
            get
            {
                if (_indexList.Count == 0)
                {
                    return
                        "No index references";
                }

                return
                    string.Join(" , ", _indexList.Select(j => j.CombinedIndexName).Distinct());
            }
        }

        public bool IsTableReferenced(string tableName)
        {
            if (tableName == null)
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            return
                _tableList.Any(j => j.IsSame(tableName));
        }

        public bool IsColumnReferenced(string columnName)
        {
            if (columnName == null)
            {
                throw new ArgumentNullException(nameof(columnName));
            }

            return
                _columnList.Any(j => j.IsSame(columnName));
        }

        public bool IsVariableReferenced(string variableName)
        {
            if (variableName == null)
            {
                throw new ArgumentNullException(nameof(variableName));
            }

            return
                _variableReferenceList.ContainsKey(variableName);
        }

        public bool IsIndexReferenced(string tableName, string indexName)
        {
            if (tableName is null)
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            if (indexName == null)
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            return
                _indexList.Any(i => i.IsSame(tableName, indexName));
        }


        public override void ExplicitVisit(AutomaticTuningDropIndexOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AutomaticTuningCreateIndexOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AutomaticTuningForceLastGoodPlanOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AutomaticTuningOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AutomaticTuningDatabaseOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(QueryStoreTimeCleanupPolicyOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(QueryStoreMaxPlansPerQueryOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(QueryStoreMaxStorageSizeOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(QueryStoreIntervalLengthOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(QueryStoreDataFlushIntervalOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(QueryStoreSizeCleanupPolicyOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(QueryStoreCapturePolicyOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(QueryStoreDesiredStateOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(QueryStoreOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(QueryStoreDatabaseOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AutomaticTuningMaintainIndexOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(FileStreamDatabaseOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(MaxSizeDatabaseOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterTableAlterIndexStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TableReplicateDistributionPolicy node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TableDistributionPolicy node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TableDistributionOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TableDataCompressionOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(FederationScheme node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ConstraintDefinition node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ChangeRetentionChangeTrackingOptionDetail node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ColumnStorageOptions node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ColumnEncryptionAlgorithmParameter node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ColumnEncryptionTypeParameter node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ColumnEncryptionKeyNameParameter node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ColumnEncryptionDefinitionParameter node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ColumnEncryptionDefinition node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(IdentityOptions node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AutoCleanupChangeTrackingOptionDetail node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ChangeTrackingOptionDetail node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ChangeTrackingDatabaseOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterDatabaseModifyNameStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterDatabaseRemoveFileStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterDatabaseRemoveFileGroupStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterDatabaseAddFileGroupStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterDatabaseAddFileStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterDatabaseRebuildLogStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterDatabaseCollateStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterDatabaseModifyFileStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(GenericConfigurationOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(OnOffPrimaryConfigurationOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DatabaseConfigurationSetOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DatabaseConfigurationClearOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterDatabaseScopedConfigurationClearStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterDatabaseScopedConfigurationSetStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterDatabaseScopedConfigurationStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterDatabaseStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(MaxDopConfigurationOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TableRoundRobinDistributionPolicy node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterDatabaseModifyFileGroupStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterDatabaseSetStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(IdentifierDatabaseOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(LiteralDatabaseOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ParameterizationDatabaseOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(WitnessDatabaseOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(PartnerDatabaseOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(PageVerifyDatabaseOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TargetRecoveryTimeDatabaseOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterDatabaseTermination node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(RecoveryDatabaseOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DelayedDurabilityDatabaseOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(HadrAvailabilityGroupDatabaseOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(HadrDatabaseOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ContainmentDatabaseOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AutoCreateStatisticsDatabaseOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(OnOffDatabaseOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DatabaseOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CursorDefaultDatabaseOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TableHashDistributionPolicy node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TableIndexOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TableIndexType node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CertificateOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateCertificateStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterCertificateStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CertificateStatementBase node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ProviderEncryptionSource node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(FileEncryptionSource node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AssemblyEncryptionSource node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateContractStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(EncryptionSource node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateRemoteServiceBindingStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(UserRemoteServiceBindingOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(OnOffRemoteServiceBindingOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(RemoteServiceBindingOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(RemoteServiceBindingStatementBase node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreatePartitionSchemeStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(PartitionParameterType node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterRemoteServiceBindingStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreatePartitionFunctionStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ContractMessage node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateCredentialStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(IPv4 node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ListenerIPEndpointProtocolOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CompressionEndpointProtocolOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(PortsEndpointProtocolOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AuthenticationEndpointProtocolOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(LiteralEndpointProtocolOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(EndpointProtocolOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CredentialStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(EndpointAffinity node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterEndpointStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateEndpointStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateAggregateStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterMessageTypeStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateMessageTypeStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(MessageTypeStatementBase node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterCredentialStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterCreateEndpointStatementBase node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(FileGroupDefinition node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateAsymmetricKeyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DbccOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(RestoreStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BackupTransactionLogStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BackupDatabaseStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BackupStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(UniqueConstraintDefinition node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(NullableConstraintDefinition node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ForeignKeyConstraintDefinition node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(RestoreOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DefaultConstraintDefinition node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CompressionPartitionRange node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DataCompressionOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TablePartitionOptionSpecifications node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(PartitionSpecifications node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TablePartitionOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TableNonClusteredIndexType node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TableClusteredIndexType node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CheckConstraintDefinition node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DbccNamedLiteral node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ScalarExpressionRestoreOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(StopRestoreOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DbccStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(InsertBulkColumnDefinition node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalTableColumnDefinition node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(OrderBulkInsertOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(LiteralBulkInsertOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BulkInsertOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(MoveRestoreOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(InsertBulkStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BulkInsertBase node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BackupRestoreFileInfo node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(MirrorToClause node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DeviceInfo node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BackupEncryptionOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BackupOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(FileStreamRestoreOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BulkInsertStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(FileGrowthFileDeclarationOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(MaxSizeFileDeclarationOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(FileNameFileDeclarationOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SearchPropertyListFullTextIndexOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(StopListFullTextIndexOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ChangeTrackingFullTextIndexOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(FullTextIndexOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateFullTextIndexStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(FullTextIndexColumn node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(LowPriorityLockWaitAbortAfterWaitOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(FullTextCatalogAndFileGroup node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(LowPriorityLockWaitMaxDurationOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(OnlineIndexLowPriorityLockWaitOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(OrderIndexOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(IgnoreDupKeyIndexOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(OnlineIndexOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(WaitAtLowPriorityOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(MaxDurationOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(IndexExpressionOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(LowPriorityLockWaitOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(IndexStateOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(EventTypeGroupContainer node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(EventGroupContainer node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropMemberAlterRoleAction node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AddMemberAlterRoleAction node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(RenameAlterRoleAction node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterRoleAction node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterRoleStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateRoleStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(RoleStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(EventTypeContainer node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterApplicationRoleStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ApplicationRoleStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ApplicationRoleOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterMasterKeyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateMasterKeyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(MasterKeyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(EventNotificationObjectScope node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateEventNotificationStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateApplicationRoleStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateServerRoleStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(IndexOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(FileGroupOrPartitionScheme node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExecuteAsClause node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateSynonymStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateTypeUddtStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateTypeUdtStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateTypeStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TryCatchStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(EnableDisableTriggerStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(QueueOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterTableTriggerModificationStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropClusteredConstraintWaitAtLowPriorityLockOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropClusteredConstraintMoveOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropClusteredConstraintValueOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropClusteredConstraintStateOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropClusteredConstraintOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(LowPriorityLockWaitTableSwitchOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(QueueStateOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(QueueValueOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateSelectiveXmlIndexStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateXmlIndexStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterIndexStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(PartitionSpecifier node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(IndexType node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(IndexStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SystemTimePeriodDefinition node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(QueueProcedureOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(IndexDefinition node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterQueueStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(QueueStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateRouteStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterRouteStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(RouteStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(RouteOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(QueueExecuteAsOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateQueueStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SoapMethod node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServerRoleStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(UserLoginOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ReconfigureStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CheckpointStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(KillStatsJobStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(KillQueryNotificationSubscriptionStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(KillStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(UseStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ThrowStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ShutdownStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(RaiseErrorStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropSchemaStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropTriggerStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropRuleStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropDefaultStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropViewStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropFunctionStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropProcedureStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(RaiseErrorLegacyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SetUserStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SetOnOffStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(NameFileDeclarationOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(FileDeclarationOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(FileDeclaration node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateDatabaseStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SetErrorLevelStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SetIdentityInsertStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SetTextSizeStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TruncateTableStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SetTransactionIsolationLevelStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SetFipsFlaggerCommand node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(GeneralSetCommand node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SetCommand node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SetOffsetsStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SetRowCountStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SetStatisticsStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(PredicateSetStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SetCommandStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropServerRoleStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropStatisticsStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(MoveToDropIndexOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CursorId node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CursorOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CursorDefinition node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DeclareCursorStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ReturnStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(UpdateStatisticsStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CursorStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateStatisticsStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(OnOffStatisticsOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(StatisticsPartitionRange node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ResampleStatisticsOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(StatisticsOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterUserStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateUserStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(UserStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(LiteralStatisticsOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(FileStreamOnDropIndexOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(OpenCursorStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CryptoMechanism node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BackwardsCompatibleDropIndexClause node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropIndexClauseBase node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropChildObjectsStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropDatabaseStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropObjectsStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CloseCursorStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropUnownedObjectStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(FetchCursorStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(FetchType node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DeallocateCursorStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CloseMasterKeyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(OpenMasterKeyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CloseSymmetricKeyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(OpenSymmetricKeyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(WhereClause node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TableSwitchOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(PayloadOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(WsdlPayloadOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterDatabaseEncryptionKeyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateDatabaseEncryptionKeyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DatabaseEncryptionKeyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(OnOffAuditTargetOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(LiteralAuditTargetOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(MaxRolloverFilesAuditTargetOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(MaxSizeAuditTargetOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AuditTargetOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(StateAuditOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(OnFailureAuditOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AuditGuidAuditOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(QueueDelayAuditOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AuditOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AuditTarget node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropServerAuditStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropDatabaseEncryptionKeyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ResourcePoolStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ResourcePoolParameter node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ResourcePoolAffinitySpecification node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropWorkloadGroupStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterWorkloadGroupStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateWorkloadGroupStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(WorkloadGroupParameter node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(WorkloadGroupImportanceParameter node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(WorkloadGroupResourceParameter node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(WorkloadGroupStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServerAuditStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropExternalResourcePoolStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateExternalResourcePoolStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalResourcePoolAffinitySpecification node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalResourcePoolParameter node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalResourcePoolStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropResourcePoolStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterResourcePoolStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateResourcePoolStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterExternalResourcePoolStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateServerAuditStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ServerAuditStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropServerAuditSpecificationStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DataModificationStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TSqlStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TSqlBatch node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TSqlScript node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(IdentifierSnippet node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TSqlStatementSnippet node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TSqlFragmentSnippet node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DataModificationSpecification node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SchemaObjectNameSnippet node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(StatementListSnippet node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BooleanExpressionSnippet node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ScalarExpressionSnippet node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(RestoreMasterKeyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BackupMasterKeyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(RestoreServiceMasterKeyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BackupServiceMasterKeyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SelectStatementSnippet node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BrokerPriorityStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(MergeStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(MergeActionClause node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServerAuditSpecificationStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateServerAuditSpecificationStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropDatabaseAuditSpecificationStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterDatabaseAuditSpecificationStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateDatabaseAuditSpecificationStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AuditActionGroupReference node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DatabaseAuditAction node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(MergeSpecification node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AuditActionSpecification node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AuditSpecificationPart node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AuditSpecificationStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateTypeTableStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(InsertMergeAction node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DeleteMergeAction node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(UpdateMergeAction node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(MergeAction node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AuditSpecificationDetail node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BrokerPriorityParameter node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateBrokerPriorityStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterBrokerPriorityStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(FailoverModeReplicaOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AvailabilityModeReplicaOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(LiteralReplicaOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AvailabilityReplicaOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AvailabilityReplica node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterAvailabilityGroupStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateAvailabilityGroupStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(PrimaryRoleReplicaOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AvailabilityGroupStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServerConfigurationSetSoftNumaStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServerConfigurationHadrClusterOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServerConfigurationSetHadrClusterStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServerConfigurationFailoverClusterPropertyOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServerConfigurationSetFailoverClusterPropertyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServerConfigurationDiagnosticsLogMaxSizeOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServerConfigurationDiagnosticsLogOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServerConfigurationSoftNumaOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServerConfigurationSetDiagnosticsLogStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SecondaryRoleReplicaOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(LiteralAvailabilityGroupOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TemporalClause node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SelectiveXmlIndexPromotedPath node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(WithinGroupClause node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(WindowDelimiter node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(WindowFrameClause node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateColumnStoreIndexStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DiskStatementOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AvailabilityGroupOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DiskStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropFederationStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterFederationStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateFederationStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropAvailabilityGroupStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterAvailabilityGroupFailoverOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterAvailabilityGroupFailoverAction node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterAvailabilityGroupAction node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(UseFederationStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BackupRestoreMasterKeyStatementBase node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServerConfigurationBufferPoolExtensionSizeOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServerConfigurationBufferPoolExtensionOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TargetDeclaration node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(EventDeclarationCompareFunctionParameter node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SourceDeclaration node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(EventDeclarationSetParameter node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(EventDeclaration node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateEventSessionStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(EventSessionStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SessionOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(EventSessionObjectName node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterCryptographicProviderStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateCryptographicProviderStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropFullTextStopListStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(FullTextStopListAction node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterFullTextStopListStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateFullTextStopListStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropBrokerPriorityStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropCryptographicProviderStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServerConfigurationBufferPoolExtensionContainerOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(EventRetentionSessionOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(LiteralSessionOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServerConfigurationSetBufferPoolExtensionStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ProcessAffinityRange node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServerConfigurationStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CellsPerObjectSpatialIndexOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(GridParameter node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(GridsSpatialIndexOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BoundingBoxParameter node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(MemoryPartitionSessionOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BoundingBoxSpatialIndexOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SpatialIndexOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateSpatialIndexStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterResourceGovernorStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropEventSessionStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterEventSessionStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(OnOffSessionOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(MaxDispatchLatencySessionOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SpatialIndexRegularOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BackupCertificateStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(OnOffDialogOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ScalarExpressionDialogOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(HavingClause node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(OutputIntoClause node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(OutputClause node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(GroupingSetsGroupingSpecification node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(GrandTotalGroupingSpecification node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(RollupGroupingSpecification node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CubeGroupingSpecification node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(IdentityFunctionCall node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CompositeGroupingSpecification node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(GroupingSpecification node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(GroupByClause node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExpressionWithSortOrder node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(GraphMatchExpression node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(GraphMatchPredicate node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BooleanIsNullExpression node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BooleanBinaryExpression node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExpressionGroupingSpecification node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BooleanComparisonExpression node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(JoinParenthesisTableReference node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ChangeTableVersionTableReference node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ChangeTableChangesTableReference node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DataModificationTableReference node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TableReferenceWithAliasAndColumns node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TableReferenceWithAlias node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TableReference node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SelectSetVariable node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(OrderByClause node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SelectElement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(FromClause node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(QueryParenthesisExpression node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(QueryExpression node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(OdbcQualifiedJoinTableReference node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(QualifiedJoin node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BooleanTernaryExpression node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BooleanParenthesisExpression node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BooleanExpression node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreationDispositionKeyOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ProviderKeyNameKeyOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(IdentityValueKeyOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlgorithmKeyOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(KeySourceKeyOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(KeyOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateSymmetricKeyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterSymmetricKeyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SymmetricKeyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AuthenticationPayloadOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(RolePayloadOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CharacterSetPayloadOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SchemaPayloadOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SessionTimeoutPayloadOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(LiteralPayloadOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(LoginTypePayloadOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(EncryptionPayloadOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BooleanNotExpression node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(FullTextCatalogStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(OnOffFullTextCatalogOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ScalarExpression node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TableSampleClause node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(UnqualifiedJoin node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(UnpivotedTableReference node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(PivotedTableReference node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ComputeFunction node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ComputeClause node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(FullTextCatalogOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(GlobalFunctionTableReference node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BinaryExpression node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ServiceContract node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServiceStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateServiceStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterCreateServiceStatementBase node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterFullTextCatalogStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateFullTextCatalogStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BuiltInFunctionTableReference node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(EnabledDisabledPayloadOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TopRowFilter node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(UnaryExpression node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropQueueStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropMessageTypeStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropEndpointStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropContractStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(RevertStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterLoginAddDropCredentialStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterLoginEnableDisableStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropRemoteServiceBindingStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterLoginOptionsStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(PasswordAlterPrincipalOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AsymmetricKeyCreateLoginSource node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CertificateCreateLoginSource node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(WindowsCreateLoginSource node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(IdentifierPrincipalOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(LiteralPrincipalOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(OnOffPrincipalOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterLoginStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(PrincipalOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropRouteStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SignatureStatementBase node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DialogOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BeginDialogStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BeginConversationTimerStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServiceMasterKeyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterAsymmetricKeyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterSchemaStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(WaitForSupportedStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropServiceStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SendStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(GetConversationGroupStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(MoveConversationStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(EndConversationStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExecuteAsStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropEventNotificationStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropSignatureStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AddSignatureStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ReceiveStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(OffsetClause node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(PasswordCreateLoginSource node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateLoginStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropMasterKeyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropUserStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropTypeStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropRoleStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropLoginStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropFullTextIndexStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropFullTextCatalogStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropSymmetricKeyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropApplicationRoleStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropAggregateStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropSynonymStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropPartitionSchemeStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropPartitionFunctionStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(VariableMethodCallTableReference node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(VariableTableReference node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BinaryQueryExpression node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropAssemblyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateLoginSource node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropAsymmetricKeyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropCredentialStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropSearchPropertyListStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropSearchPropertyListAction node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AddSearchPropertyListAction node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SearchPropertyListAction node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterSearchPropertyListStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateSearchPropertyListStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterColumnAlterFullTextIndexAction node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropCertificateStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AddAlterFullTextIndexAction node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SetSearchPropertyListAlterFullTextIndexAction node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SetStopListAlterFullTextIndexAction node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SimpleAlterFullTextIndexAction node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterFullTextIndexAction node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterFullTextIndexStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterPartitionSchemeStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterPartitionFunctionStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropAlterFullTextIndexAction node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterTableSwitchStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SizeFileDeclarationOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CompressionDelayIndexOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ForClause node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(OdbcLiteral node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(LiteralRange node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(StatementWithCtesAndXmlNamespaces node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ValueExpression node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(UserDefinedTypePropertyAccess node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(OptionValue node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(FullTextPredicate node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(OnOffOptionValue node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(InPredicate node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(LiteralOptionValue node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(GlobalVariableExpression node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(LikePredicate node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(IdentifierOrValueExpression node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExistsPredicate node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(IdentifierOrScalarExpression node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SubqueryComparisonPredicate node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ParenthesisExpression node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(InlineDerivedTable node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(QueryDerivedTable node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(MaxLiteral node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); }
        public override void ExplicitVisit(DefaultLiteral node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BrowseForClause node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(IdentifierLiteral node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(OptimizeForOptimizerHint node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(RowValue node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ForceSeekTableHint node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(PrintStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TableHintsOptimizerHint node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(UpdateCall node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TSEqualCall node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(LiteralOptimizerHint node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(PrimaryExpression node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(OptimizerHint node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(Literal node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(NextValueForExpression node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(UpdateForClause node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(NumericLiteral node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(JsonForClauseOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(RealLiteral node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(JsonForClause node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(MoneyLiteral node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(XmlForClauseOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BinaryLiteral node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(StringLiteral node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(XmlForClause node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(NullLiteral node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ReadOnlyForClause node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(IntegerLiteral node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExecuteInsertSource node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(LiteralTableHint node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SequenceOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropColumnMasterKeyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DataTypeReference node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ColumnEncryptionKeyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateColumnEncryptionKeyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TableValuedFunctionReturnType node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterColumnEncryptionKeyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(FunctionReturnType node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropColumnEncryptionKeyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(WithCtesAndXmlNamespaces node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ColumnEncryptionKeyValue node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ColumnEncryptionKeyValueParameter node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CommonTableExpression node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ColumnMasterKeyNameParameter node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(XmlNamespacesAliasElement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ColumnEncryptionAlgorithmNameParameter node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(XmlNamespacesDefaultElement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(EncryptedValueParameter node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalTableStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(XmlNamespacesElement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalTableOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(XmlNamespaces node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalTableLiteralOrIdentifierOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExecuteAsFunctionOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ParameterizedDataTypeReference node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ColumnMasterKeyPathParameter node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SqlDataTypeReference node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ColumnMasterKeyStoreProviderNameParameter node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(IndexTableHint node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DataTypeSequenceOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TableHint node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ScalarExpressionSequenceOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SchemaObjectFunctionTableReference node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateSequenceStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterSequenceStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropSequenceStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DeclareTableVariableStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SecurityPolicyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SequenceStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SecurityPolicyOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SelectFunctionReturnType node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateSecurityPolicyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterSecurityPolicyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ScalarFunctionReturnType node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropSecurityPolicyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(XmlDataTypeReference node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateColumnMasterKeyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(UserDataTypeReference node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ColumnMasterKeyParameter node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SecurityPredicateAction node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SelectInsertSource node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(UseHintList node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ValuesInsertSource node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(IfStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(LeftFunctionCall node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(LabelStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(UserDefinedTypeCallTarget node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(MultiPartIdentifierCallTarget node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ChildObjectName node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExpressionCallTarget node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ProcedureParameter node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CallTarget node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TransactionStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(WhileStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(FunctionCall node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DeleteStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AtTimeZoneCall node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(UpdateDeleteSpecificationBase node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TryCastCall node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DeleteSpecification node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(InsertStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CastCall node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(InsertSpecification node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TryParseCall node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(RightFunctionCall node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(GoToStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(PartitionFunctionCall node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(IdentifierAtomicBlockOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AtomicBlockOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(OnOffAtomicBlockOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BeginEndAtomicBlockStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BeginTransactionStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BreakStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BeginEndBlockStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ColumnWithSortOrder node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterFunctionStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CommitTransactionStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(OdbcConvertSpecification node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(UpdateStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(RollbackTransactionStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExtractFromExpression node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ContinueStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(OdbcFunctionCall node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateDefaultStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ScalarSubquery node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateFunctionStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateOrAlterFunctionStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ParameterlessCall node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateRuleStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(OverClause node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SaveTransactionStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ParseCall node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(UpdateSpecification node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateSchemaStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(FullTextTableReference node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(GrantStatement80 node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(IIfCall node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DenyStatement80 node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CoalesceExpression node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(RevokeStatement80 node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SecurityElement80 node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(NullIfExpression node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CommandSecurityElement80 node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SearchedCaseExpression node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(PrivilegeSecurityElement80 node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SecurityStatementBody80 node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SimpleCaseExpression node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SecurityUserClause80 node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CaseExpression node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SqlCommandIdentifier node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SearchedWhenClause node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SetClause node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SimpleWhenClause node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AssignmentSetClause node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(FunctionCallSetClause node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(WhenClause node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(InsertSource node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(VariableValuePair node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(Privilege80 node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalTableDistributionOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SecurityPrincipal node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SecurityTargetObjectName node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TryConvertCall node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(WaitForStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ConvertCall node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ReadTextStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SchemaDeclarationItemOpenjson node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(UpdateTextStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(WriteTextStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SchemaDeclarationItem node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TextModificationStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AdHocTableReference node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(LineNoStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SemanticTableReference node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(OpenQueryTableReference node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(GrantStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(BulkOpenRowset node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DenyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(InternalOpenRowset node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(RevokeStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(OpenRowsetTableReference node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterAuthorizationStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(Permission node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(OpenJsonTableReference node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SecurityTargetObject node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(OpenXmlTableReference node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SecurityStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalTableRejectTypeOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SchemaObjectNameOrValueExpression node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(LiteralAtomicBlockOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateProcedureStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ResultColumnDefinition node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropExternalDataSourceStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExecuteAsTriggerOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(RemoteDataArchiveAlterTableOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AssemblyOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterExternalDataSourceStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(InlineResultSetDefinition node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateOrAlterProcedureStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExecutableProcedureReference node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterTableSetStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(RemoteDataArchiveDatabaseOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TableOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(OnOffAssemblyOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ProcedureReference node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExecuteParameter node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(RemoteDataArchiveDatabaseSetting node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalDataSourceLiteralOrIdentifierOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ResultSetDefinition node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterTableFileTableNamespaceStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(PermissionSetAssemblyOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExecutableStringList node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(MethodSpecifier node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(RemoteDataArchiveDbServerSetting node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalDataSourceOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TriggerObject node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateExternalDataSourceStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterAssemblyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(RemoteDataArchiveTableOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalFileFormatStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateExternalFileFormatStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(FileTableDirectoryTableOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateTriggerStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(FileTableCollateFileNameTableOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateOrAlterTriggerStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropExternalFileFormatStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExecuteContext node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalFileFormatContainerOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(FileTableConstraintNameTableOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TriggerStatementBody node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalFileFormatUseDefaultTypeOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(FileStreamOnTableOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AssemblyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExecutableEntity node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExecuteSpecification node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterTriggerStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(MemoryOptimizedTableOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalFileFormatLiteralOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateAssemblyStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(LockEscalationTableOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalFileFormatOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DurabilityTableOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SchemaObjectResultSetDefinition node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TriggerAction node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterProcedureStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ProcedureReferenceName node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ResultSetsExecuteOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalDataSourceStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TriggerOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ProcedureStatementBodyBase node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(RemoteDataArchiveDbFederatedServiceAccountSetting node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AddFileSpec node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateViewStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExecuteAsProcedureOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateXmlSchemaCollectionStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExecuteStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(FunctionStatementBody node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalTableShardedDistributionPolicy node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateExternalTableStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterTableRebuildStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterXmlSchemaCollectionStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(StatementList node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(FunctionOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalTableReplicatedDistributionPolicy node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(RetentionPeriodDefinition node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalTableRoundRobinDistributionPolicy node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateOrAlterViewStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropXmlSchemaCollectionStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterTableChangeTrackingModificationStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterTableConstraintModificationStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(RemoteDataArchiveDbCredentialSetting node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SystemVersioningTableOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AssemblyName node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ViewStatementBody node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalTableDistributionPolicy node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ViewOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ProcedureStatementBody node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterTableAlterPartitionStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ProcedureOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(DropExternalTableStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AdHocDataSource node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterViewStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ExecuteOption node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterTableStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ColumnDefinitionBase node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(JoinTableReference node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SchemaObjectName node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(QuerySpecification node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SelectScalarExpression node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(TableDefinition node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(SelectStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(ColumnReferenceExpression node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }
        public override void ExplicitVisit(Microsoft.SqlServer.TransactSql.ScriptDom.Identifier node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }


        #region no need to modify, because its child nodes do the work as it should

        public override void ExplicitVisit(DeclareVariableStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }

        public override void ExplicitVisit(SetVariableStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }

        public override void ExplicitVisit(DropIndexStatement node) { Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString()); node.AcceptChildren(this); }

        #endregion


        public override void ExplicitVisit(CreateIndexStatement node)
        {
            var table = new SqlServerTableName(node.OnName);
            _tableList.Add(table);

            var index = new SqlServerIndexName(table, node.Name.Value);
            _indexList.Add(index);

            foreach (var ncolumn in node.Columns)
            {
                var column = new SqlServerColumnName(ncolumn.Column.MultiPartIdentifier.ToSqlString());
                _columnList.Add(column);
            }
            foreach (var ncolumn in node.IncludeColumns)
            {
                var column = new SqlServerColumnName(ncolumn.MultiPartIdentifier.ToSqlString());
                _columnList.Add(column);
            }
        }



        public override void ExplicitVisit(DropIndexClause node)
        {
            var table = new SqlServerTableName(node.Object);
            _tableList.Add(table);

            var index = new SqlServerIndexName(table, node.Index.Value);
            _indexList.Add(index);
        }


        public override void ExplicitVisit(DeclareVariableElement node)
        {
            AppendNewVariableReference(node.VariableName.Value);
        }


        //*/

        public override void ExplicitVisit(SelectStarExpression node)
        {
            IsStarReferenced = true;

            _columnList.Add(
                new SqlServerColumnName(
                    "*"
                    )
                );
            //Debug.WriteLine(node.GetType().Name.PadRight(40) + node.ToSourceSqlString());

            node.AcceptChildren(this);
        }


        public override void ExplicitVisit(CreateTableStatement node)
        {
            var tableName = new SqlServerTableName(
                node.SchemaObjectName
                );

            _tableList.Add(tableName);

            node.AcceptChildren(this);
        }


        public override void ExplicitVisit(MultiPartIdentifier node)
        {
            var columnName = node.Identifiers.Last().ToSourceSqlString();

            _columnList.Add(
                new SqlServerColumnName(
                    columnName
                    )
                );

            node.AcceptChildren(this);
        }


        public override void ExplicitVisit(NamedTableReference node)
        {
            var tableName = new SqlServerTableName(
                node.SchemaObject
                );

            _tableList.Add(tableName);

            node.AcceptChildren(this);
        }

        public override void ExplicitVisit(DropTableStatement node)
        {
            foreach (SchemaObjectName o in node.Objects)
            {
                var tableName = new SqlServerTableName(
                    o
                    );

                _tableList.Add(tableName);
            }

            node.AcceptChildren(this);
        }

        public override void ExplicitVisit(ColumnDefinition node)
        {
            _columnList.Add(
                new SqlServerColumnName(
                    node.ColumnIdentifier.Value
                    )
                );

            node.AcceptChildren(this);
        }

        public override void ExplicitVisit(AlterTableAlterColumnStatement node)
        {
            var tableName = new SqlServerTableName(
                node.SchemaObjectName
                );

            _tableList.Add(tableName);

            _columnList.Add(
                new SqlServerColumnName(
                    node.ColumnIdentifier.Value
                    )
                );

            node.AcceptChildren(this);
        }

        public override void ExplicitVisit(AlterTableAddTableElementStatement node)
        {
            var tableName = new SqlServerTableName(
                node.SchemaObjectName
                );

            _tableList.Add(tableName);

            node.AcceptChildren(this);
        }

        public override void ExplicitVisit(AlterTableDropTableElementStatement node)
        {
            var tableName = new SqlServerTableName(
                node.SchemaObjectName
                );

            _tableList.Add(tableName);

            //_columnList.Add(
            //    new SqlServerColumnName(
            //        node.ColumnIdentifier.Value
            //        )
            //    );

            node.AcceptChildren(this);
        }

        public override void ExplicitVisit(AlterTableDropTableElement node)
        {
            _columnList.Add(
                new SqlServerColumnName(
                    node.Name.Value
                    )
                );

            node.AcceptChildren(this);
        }

        public override void ExplicitVisit(DeclareTableVariableBody node)
        {
            var tableName = new SqlServerTableVariableName(
                node.VariableName.Value
                );

            _tableList.Add(tableName);

            node.AcceptChildren(this);
        }

        public override void ExplicitVisit(VariableReference node)
        {
            AppendNewVariableReference(node.Name);

            node.AcceptChildren(this);
        }





        private void AppendNewVariableReference(string variableName)
        {
            if (!_variableReferenceList.ContainsKey(variableName))
            {
                _variableReferenceList[variableName] = new SqlServerValidator.Visitor.VariableRef.VariableRef(variableName);
            }

            _variableReferenceList[variableName].IncrementReferenceCount();
        }
    }

}
