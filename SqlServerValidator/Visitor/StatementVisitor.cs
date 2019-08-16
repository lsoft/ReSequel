using System;
using System.Collections.Generic;
using System.Linq;
using Main.Inclusion.Validated.Result;
using Main.Sql;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServerValidator.Visitor.Known;

namespace SqlServerValidator.Visitor
{
    public class StatementVisitor : TSqlFragmentVisitor
    {
        public const string SqlServerSpecificVariablePrefix = "@@";
        public const string SqlServerVariablePrefix = "@";
        public const string SqlServerTempTablePrefix1 = "##";
        public const string SqlServerTempTablePrefix2 = "#";

        private readonly ISqlValidator _sqlValidator;
        private readonly IDuplicateProcessor _duplicateProcessor;
        private readonly List<IKnownVariable> _knownVariables;

        private ButcherVisitor _butcher;
        private IModifiedValidationResult _result;

        private StatementVisitor(
            StatementVisitor visitor
            )
        {
            if (visitor == null)
            {
                throw new ArgumentNullException(nameof(visitor));
            }

            this._sqlValidator = visitor._sqlValidator; //of course, validate against same RDBMS
            this._duplicateProcessor = visitor._duplicateProcessor; //we also need duplicaProcessor
            this._knownVariables = visitor._knownVariables; //share same known vars container!
            this._butcher = visitor._butcher; //share same butcher!
        }

        public StatementVisitor(
            ISqlValidator sqlValidator,
            IDuplicateProcessor duplicateProcessor
            )
        {
            if (duplicateProcessor == null)
            {
                throw new ArgumentNullException(nameof(duplicateProcessor));
            }

            if (sqlValidator == null)
            {
                throw new ArgumentNullException(nameof(sqlValidator));
            }

            _sqlValidator = sqlValidator;
            _duplicateProcessor = duplicateProcessor;
            _knownVariables = new List<IKnownVariable>();
        }

        public IModifiedValidationResult ProcessNextStatement(
            TSqlFragment statement
            )
        {
            if (statement == null)
            {
                throw new ArgumentNullException(nameof(statement));
            }

            //process the statement with the butcher
            _butcher = new ButcherVisitor();
            statement.Accept(_butcher);

            //prepare this visitor
            _result = new NotImplementedValidationResult(
                "Not implemented yet"
                );

            //process the statement with this visitor
            statement.Accept(this);

            //gathering the result
            var result = this._result.WithCarveResult(_butcher);

            return
                result;
        }

        private bool TryToFoundTempTableReference(
            TSqlFragment statement,
            out IModifiedValidationResult visitResult
            )
        {
            var statementSql = statement.ToSourceSqlString();

            try
            {
                //var visitor = new ButcherVisitor();
                //statement.Accept(visitor);

                var tempTableList = _butcher.TempTableList;
                if (tempTableList.Count > 0)
                {
                    visitResult = ValidationResult.CannotValidate(
                        statementSql, 
                        statementSql,
                        "Found temp tables: " + string.Join(",", tempTableList.Select(j => j.FullTableName))
                        );

                    return true; //found
                }

                //not found
                visitResult = null;

                return
                    false;
            }
            catch (Exception excp)
            {
                visitResult = ValidationResult.CannotValidate(statementSql, statementSql, excp.Message);
                return true; //found
            }
        }

        private IModifiedValidationResult ExecuteDefaultProcessing(
            TSqlFragment statement
            )
        {
            if (statement == null)
            {
                throw new ArgumentNullException(nameof(statement));
            }

            var sql = _duplicateProcessor.DetermineDuplicates(
                statement,
                _knownVariables
                );

            var result = ExecuteDefaultProcessing(
                sql
                );

            if (!result.IsSuccess)
            {
                result = result.WithNewFullSqlBody(statement.ToSourceSqlString());
            }

            return
                result;
        }

        private IModifiedValidationResult ExecuteDefaultProcessing(
            string statementSql
            )
        {

            if (statementSql == null)
            {
                throw new ArgumentNullException(nameof(statementSql));
            }

            var declarations = string.Join(Environment.NewLine, _knownVariables.ConvertAll(j => j.ToSqlDeclaration()));

            IModifiedValidationResult result;

            string errorMessage;
            if (_sqlValidator.TryCheckSql(declarations + Environment.NewLine + statementSql, out errorMessage))
            {
                result = ValidationResult.Success(statementSql, statementSql);
            }
            else
            {
                result = ValidationResult.Error(statementSql, statementSql, errorMessage);
            }

            return
                result;
        }

        //public IValidationResult2 Visit(SqlAlterFunctionStatement statement)
        //{
        //    //return new NotImplementedValidationResult(statement);
        //}


        public override void ExplicitVisit(QuerySpecification node)
        {
            IModifiedValidationResult tempTableVisitReturn;
            if (TryToFoundTempTableReference(node, out tempTableVisitReturn))
            {
                this._result = tempTableVisitReturn;
                return;
            }

            this._result = ExecuteDefaultProcessing(node);

            //node.AcceptChildren(this);
        }

        public override void ExplicitVisit(IfStatement node)
        {
            var statementSql = node.ToSourceSqlString();

            var predicateVisitor = new StatementVisitor(this);
            //var predicateVisitorResult = predicateVisitor.ProcessNextStatement(node.Predicate);
            node.Predicate.Accept(predicateVisitor);
            if (predicateVisitor._result != null && !predicateVisitor._result.IsSuccess)
            {
                this._result = predicateVisitor._result.WithNewFullSqlBody(statementSql);
                return;
            }

            //BooleanExpression predicate = node.Predicate;

            //var sqlBody = predicate.FixDuplicates(
            //    _knownVariables
            //    )
            //    .Trim('(', ')', ' ')
            //    ;

            ////sqlBody = string.Format(
            ////    "if {0} print 1;",
            ////    sqlBody
            ////    );

            //var conditionResult = ExecuteDefaultProcessing(
            //    sqlBody
            //    );
            //if (!conditionResult.IsSuccess)
            //{
            //    this.Result = conditionResult.WithNewFullSqlBody(statementSql);
            //    return;
            //}

            var trueStatement = node.ThenStatement;
            var falseStatement = node.ElseStatement;

            var trueVisitor = new StatementVisitor(this);
            //var trueVisitorResult = trueVisitor.ProcessNextStatement(node.Predicate);
            trueStatement.Accept(trueVisitor);
            if (!trueVisitor._result.IsSuccess || falseStatement == null)
            {
                this._result = trueVisitor._result.WithNewFullSqlBody(statementSql);
                return;
            }

            //this._knownVariables.AddRange(trueVisitor._knownVariables);

            var falseVisitor = new StatementVisitor(this);
            //var falseVisitorResult = falseVisitor.ProcessNextStatement(node.Predicate);
            falseStatement.Accept(falseVisitor);
            if (!falseVisitor._result.IsSuccess)
            {
                this._result = falseVisitor._result.WithNewFullSqlBody(statementSql);
                return;
            }

            this._result = ValidationResult.Success(statementSql, statementSql);


            //this.Result = ExecuteDefaultProcessing(node);

            //node.AcceptChildren(this);
        }

        public override void ExplicitVisit(ReturnStatement node)
        {
            this._result = ExecuteDefaultProcessing(node);

            //node.AcceptChildren(this);
        }

        public override void ExplicitVisit(RaiseErrorStatement node)
        {
            this._result = ExecuteDefaultProcessing(node);

            //node.AcceptChildren(this);
        }

        public override void ExplicitVisit(SetErrorLevelStatement node)
        {
            this._result = ExecuteDefaultProcessing(node);

            //node.AcceptChildren(this);
        }

        public override void ExplicitVisit(SetIdentityInsertStatement node)
        {
            this._result = ExecuteDefaultProcessing(node);

            //node.AcceptChildren(this);
        }

        public override void ExplicitVisit(SetTextSizeStatement node)
        {
            this._result = ExecuteDefaultProcessing(node);

            //node.AcceptChildren(this);
        }

        public override void ExplicitVisit(OpenCursorStatement node)
        {
            this._result = ExecuteDefaultProcessing(node);

            //node.AcceptChildren(this);
        }

        public override void ExplicitVisit(FetchCursorStatement node)
        {
            this._result = ExecuteDefaultProcessing(node);

            //node.AcceptChildren(this);
        }

        public override void ExplicitVisit(CloseCursorStatement node)
        {
            this._result = ExecuteDefaultProcessing(node);

            //node.AcceptChildren(this);
        }

        public override void ExplicitVisit(DeallocateCursorStatement node)
        {
            this._result = ExecuteDefaultProcessing(node);

            //node.AcceptChildren(this);
        }

        public override void ExplicitVisit(PredicateSetStatement node)
        {
            this._result = ExecuteDefaultProcessing(node);

            //node.AcceptChildren(this);
        }

        public override void ExplicitVisit(PrintStatement node)
        {
            this._result = ExecuteDefaultProcessing(node);

            //node.AcceptChildren(this);
        }

        public override void ExplicitVisit(SetTransactionIsolationLevelStatement node)
        {
            this._result = ExecuteDefaultProcessing(node);

            //node.AcceptChildren(this);
        }

        public override void ExplicitVisit(CommitTransactionStatement node)
        {
            this._result = ExecuteDefaultProcessing(node);

            //node.AcceptChildren(this);
        }

        public override void ExplicitVisit(RollbackTransactionStatement node)
        {
            this._result = ExecuteDefaultProcessing(node);

            //node.AcceptChildren(this);
        }

        public override void ExplicitVisit(BeginTransactionStatement node)
        {
            this._result = ExecuteDefaultProcessing(node);

            //node.AcceptChildren(this);
        }

        public override void ExplicitVisit(SetVariableStatement node)
        {
            this._result = ExecuteDefaultProcessing(node);

            //node.AcceptChildren(this);
        }

        public override void ExplicitVisit(MergeStatement node)
        {
            IModifiedValidationResult tempTableVisitReturn;
            if (TryToFoundTempTableReference(node, out tempTableVisitReturn))
            {
                this._result = tempTableVisitReturn;
                return;
            }

            this._result = ExecuteDefaultProcessing(node);

            //node.AcceptChildren(this);
        }

        public override void ExplicitVisit(DeclareVariableStatement node)
        {
            this._result = ExecuteDefaultProcessing(node);

            //node.AcceptChildren(this);

            if (this._result.IsSuccess)
            {
                var newDeclarationTerms = KnownVariable.Parse(node);
                if (newDeclarationTerms != null)
                {
                    _knownVariables.AddRange(newDeclarationTerms);
                }
            }
        }

        public override void ExplicitVisit(AlterIndexStatement node)
        {
            this._result = ExecuteDefaultProcessing(node);

            //node.AcceptChildren(this);
        }

        public override void ExplicitVisit(AlterTableAlterIndexStatement node)
        {
            this._result = ExecuteDefaultProcessing(node);

            //node.AcceptChildren(this);
        }

        public override void ExplicitVisit(CreateIndexStatement node)
        {
            this._result = ExecuteDefaultProcessing(node);

            //node.AcceptChildren(this);
        }

        public override void ExplicitVisit(DeclareTableVariableStatement node)
        {
            this._result = ExecuteDefaultProcessing(node);

            //node.AcceptChildren(this);

            if (this._result.IsSuccess)
            {
                var newDeclarationTerm = KnownTableVariable.Parse(node);
                if (newDeclarationTerm != null)
                {
                    _knownVariables.Add(newDeclarationTerm);
                }
            }
        }

        public override void ExplicitVisit(TruncateTableStatement node)
        {
            this._result = ExecuteDefaultProcessing(node);

            //node.AcceptChildren(this);
        }

        public override void ExplicitVisit(CreateTableStatement node)
        {
            this._result = ExecuteDefaultProcessing(node);

            //node.AcceptChildren(this);
        }

        public override void ExplicitVisit(ExecuteStatement node)
        {
            this._result = ExecuteDefaultProcessing(node);

            //node.AcceptChildren(this);
        }

        public override void ExplicitVisit(DropTableStatement node)
        {
            this._result = ExecuteDefaultProcessing(node);

            //node.AcceptChildren(this);
        }

        public override void ExplicitVisit(DeleteStatement node)
        {
            IModifiedValidationResult tempTableVisitReturn;
            if (TryToFoundTempTableReference(node, out tempTableVisitReturn))
            {
                this._result = tempTableVisitReturn;
                return;
            }

            this._result = ExecuteDefaultProcessing(node);

            //node.AcceptChildren(this);
        }

        public override void ExplicitVisit(InsertStatement node)
        {
            IModifiedValidationResult tempTableVisitReturn;
            if (TryToFoundTempTableReference(node, out tempTableVisitReturn))
            {
                this._result = tempTableVisitReturn;
                return;
            }

            this._result = ExecuteDefaultProcessing(node);

            //node.AcceptChildren(this);
        }

        public override void ExplicitVisit(UpdateStatement node)
        {
            IModifiedValidationResult tempTableVisitReturn;
            if (TryToFoundTempTableReference(node, out tempTableVisitReturn))
            {
                this._result = tempTableVisitReturn;
                return;
            }

            this._result = ExecuteDefaultProcessing(node);

            //node.AcceptChildren(this);
        }

        public override void ExplicitVisit(SelectStatement node)
        {
            IModifiedValidationResult tempTableVisitReturn;
            if (TryToFoundTempTableReference(node, out tempTableVisitReturn))
            {
                this._result = tempTableVisitReturn;
                return;
            }

            this._result = ExecuteDefaultProcessing(node);

            //node.AcceptChildren(this);
        }

        public override void ExplicitVisit(AutomaticTuningDropIndexOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AutomaticTuningCreateIndexOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AutomaticTuningForceLastGoodPlanOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AutomaticTuningOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AutomaticTuningDatabaseOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(QueryStoreTimeCleanupPolicyOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(QueryStoreMaxPlansPerQueryOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(QueryStoreMaxStorageSizeOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(QueryStoreIntervalLengthOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(QueryStoreDataFlushIntervalOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(QueryStoreSizeCleanupPolicyOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(QueryStoreCapturePolicyOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(QueryStoreDesiredStateOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(QueryStoreOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(QueryStoreDatabaseOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AutomaticTuningMaintainIndexOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(FileStreamDatabaseOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(MaxSizeDatabaseOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TableReplicateDistributionPolicy node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TableDistributionPolicy node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TableDistributionOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TableDataCompressionOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(FederationScheme node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ConstraintDefinition node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ChangeRetentionChangeTrackingOptionDetail node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ColumnStorageOptions node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ColumnEncryptionAlgorithmParameter node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ColumnEncryptionTypeParameter node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ColumnEncryptionKeyNameParameter node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ColumnEncryptionDefinitionParameter node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ColumnEncryptionDefinition node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ColumnDefinition node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterTableAlterColumnStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(IdentityOptions node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AutoCleanupChangeTrackingOptionDetail node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ChangeTrackingOptionDetail node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ChangeTrackingDatabaseOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterDatabaseModifyNameStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterDatabaseRemoveFileStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterDatabaseRemoveFileGroupStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterDatabaseAddFileGroupStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterDatabaseAddFileStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterDatabaseRebuildLogStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterDatabaseCollateStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterDatabaseModifyFileStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(GenericConfigurationOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(OnOffPrimaryConfigurationOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DatabaseConfigurationSetOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DatabaseConfigurationClearOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterDatabaseScopedConfigurationClearStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterDatabaseScopedConfigurationSetStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterDatabaseScopedConfigurationStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterDatabaseStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(MaxDopConfigurationOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TableRoundRobinDistributionPolicy node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterDatabaseModifyFileGroupStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterDatabaseSetStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(IdentifierDatabaseOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(LiteralDatabaseOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ParameterizationDatabaseOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(WitnessDatabaseOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(PartnerDatabaseOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(PageVerifyDatabaseOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TargetRecoveryTimeDatabaseOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterDatabaseTermination node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(RecoveryDatabaseOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DelayedDurabilityDatabaseOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(HadrAvailabilityGroupDatabaseOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(HadrDatabaseOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ContainmentDatabaseOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AutoCreateStatisticsDatabaseOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(OnOffDatabaseOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DatabaseOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CursorDefaultDatabaseOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TableHashDistributionPolicy node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TableIndexOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TableIndexType node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CertificateOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateCertificateStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterCertificateStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CertificateStatementBase node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ProviderEncryptionSource node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(FileEncryptionSource node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AssemblyEncryptionSource node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateContractStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(EncryptionSource node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateRemoteServiceBindingStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(UserRemoteServiceBindingOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(OnOffRemoteServiceBindingOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(RemoteServiceBindingOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(RemoteServiceBindingStatementBase node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreatePartitionSchemeStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(PartitionParameterType node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterRemoteServiceBindingStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreatePartitionFunctionStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ContractMessage node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateCredentialStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(IPv4 node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ListenerIPEndpointProtocolOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CompressionEndpointProtocolOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(PortsEndpointProtocolOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AuthenticationEndpointProtocolOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(LiteralEndpointProtocolOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(EndpointProtocolOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CredentialStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(EndpointAffinity node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterEndpointStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateEndpointStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateAggregateStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterMessageTypeStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateMessageTypeStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(MessageTypeStatementBase node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterCredentialStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterCreateEndpointStatementBase node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(FileGroupDefinition node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateAsymmetricKeyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DbccOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(RestoreStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(BackupTransactionLogStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(BackupDatabaseStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(BackupStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(UniqueConstraintDefinition node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(NullableConstraintDefinition node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ForeignKeyConstraintDefinition node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(RestoreOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DefaultConstraintDefinition node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CompressionPartitionRange node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DataCompressionOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TablePartitionOptionSpecifications node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(PartitionSpecifications node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TablePartitionOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TableNonClusteredIndexType node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TableClusteredIndexType node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CheckConstraintDefinition node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DbccNamedLiteral node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ScalarExpressionRestoreOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(StopRestoreOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DbccStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(InsertBulkColumnDefinition node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalTableColumnDefinition node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ColumnDefinitionBase node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(OrderBulkInsertOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(LiteralBulkInsertOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(BulkInsertOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(MoveRestoreOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(InsertBulkStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(BulkInsertBase node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(BackupRestoreFileInfo node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(MirrorToClause node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DeviceInfo node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(BackupEncryptionOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(BackupOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(FileStreamRestoreOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(BulkInsertStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(FileGrowthFileDeclarationOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(MaxSizeFileDeclarationOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(FileNameFileDeclarationOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SearchPropertyListFullTextIndexOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(StopListFullTextIndexOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ChangeTrackingFullTextIndexOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(FullTextIndexOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateFullTextIndexStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(FullTextIndexColumn node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(LowPriorityLockWaitAbortAfterWaitOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(FullTextCatalogAndFileGroup node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(LowPriorityLockWaitMaxDurationOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(OnlineIndexLowPriorityLockWaitOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(OrderIndexOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(IgnoreDupKeyIndexOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(OnlineIndexOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(WaitAtLowPriorityOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(MaxDurationOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(IndexExpressionOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(LowPriorityLockWaitOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(IndexStateOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(EventTypeGroupContainer node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(EventGroupContainer node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropMemberAlterRoleAction node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AddMemberAlterRoleAction node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(RenameAlterRoleAction node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterRoleAction node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterRoleStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateRoleStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(RoleStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(EventTypeContainer node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterApplicationRoleStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ApplicationRoleStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ApplicationRoleOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterMasterKeyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateMasterKeyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(MasterKeyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(EventNotificationObjectScope node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateEventNotificationStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateApplicationRoleStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateServerRoleStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(IndexOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(FileGroupOrPartitionScheme node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExecuteAsClause node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateSynonymStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateTypeUddtStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateTypeUdtStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateTypeStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TryCatchStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(EnableDisableTriggerStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(QueueOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterTableTriggerModificationStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterTableDropTableElement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropClusteredConstraintWaitAtLowPriorityLockOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropClusteredConstraintMoveOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropClusteredConstraintValueOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropClusteredConstraintStateOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropClusteredConstraintOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(LowPriorityLockWaitTableSwitchOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterTableDropTableElementStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(QueueStateOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(QueueValueOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateSelectiveXmlIndexStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateXmlIndexStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(PartitionSpecifier node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(IndexType node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(IndexStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SystemTimePeriodDefinition node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(QueueProcedureOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(IndexDefinition node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterQueueStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(QueueStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateRouteStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterRouteStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(RouteStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(RouteOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(QueueExecuteAsOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateQueueStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SoapMethod node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServerRoleStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(UserLoginOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ReconfigureStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CheckpointStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(KillStatsJobStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(KillQueryNotificationSubscriptionStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(KillStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(UseStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ThrowStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ShutdownStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropSchemaStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropTriggerStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropRuleStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropDefaultStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropViewStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropFunctionStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropProcedureStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(RaiseErrorLegacyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SetUserStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SetOnOffStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(NameFileDeclarationOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(FileDeclarationOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(FileDeclaration node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateDatabaseStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SetFipsFlaggerCommand node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(GeneralSetCommand node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SetCommand node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SetOffsetsStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SetRowCountStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SetStatisticsStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SetCommandStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropServerRoleStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropStatisticsStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(MoveToDropIndexOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CursorId node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CursorOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CursorDefinition node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DeclareCursorStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(UpdateStatisticsStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CursorStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateStatisticsStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(OnOffStatisticsOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(StatisticsPartitionRange node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ResampleStatisticsOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(StatisticsOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterUserStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateUserStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(UserStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(LiteralStatisticsOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(FileStreamOnDropIndexOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CryptoMechanism node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropIndexClause node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(BackwardsCompatibleDropIndexClause node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropIndexClauseBase node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropIndexStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropChildObjectsStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropDatabaseStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropObjectsStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropUnownedObjectStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(FetchType node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CloseMasterKeyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(OpenMasterKeyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CloseSymmetricKeyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(OpenSymmetricKeyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(WhereClause node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TableSwitchOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(PayloadOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(WsdlPayloadOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterDatabaseEncryptionKeyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateDatabaseEncryptionKeyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DatabaseEncryptionKeyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(OnOffAuditTargetOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(LiteralAuditTargetOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(MaxRolloverFilesAuditTargetOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(MaxSizeAuditTargetOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AuditTargetOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(StateAuditOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(OnFailureAuditOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AuditGuidAuditOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(QueueDelayAuditOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AuditOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AuditTarget node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropServerAuditStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropDatabaseEncryptionKeyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ResourcePoolStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ResourcePoolParameter node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ResourcePoolAffinitySpecification node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropWorkloadGroupStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterWorkloadGroupStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateWorkloadGroupStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(WorkloadGroupParameter node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(WorkloadGroupImportanceParameter node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(WorkloadGroupResourceParameter node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(WorkloadGroupStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServerAuditStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropExternalResourcePoolStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateExternalResourcePoolStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalResourcePoolAffinitySpecification node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalResourcePoolParameter node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalResourcePoolStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropResourcePoolStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterResourcePoolStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateResourcePoolStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterExternalResourcePoolStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateServerAuditStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ServerAuditStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropServerAuditSpecificationStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DataModificationStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TSqlStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TSqlBatch node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TSqlScript node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(IdentifierSnippet node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TSqlStatementSnippet node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TSqlFragmentSnippet node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DataModificationSpecification node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SchemaObjectNameSnippet node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(StatementListSnippet node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(BooleanExpressionSnippet node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ScalarExpressionSnippet node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(RestoreMasterKeyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(BackupMasterKeyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(RestoreServiceMasterKeyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(BackupServiceMasterKeyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SelectStatementSnippet node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(BrokerPriorityStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(MergeActionClause node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServerAuditSpecificationStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateServerAuditSpecificationStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropDatabaseAuditSpecificationStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterDatabaseAuditSpecificationStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateDatabaseAuditSpecificationStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AuditActionGroupReference node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DatabaseAuditAction node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(MergeSpecification node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AuditActionSpecification node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AuditSpecificationPart node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AuditSpecificationStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateTypeTableStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(InsertMergeAction node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DeleteMergeAction node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(UpdateMergeAction node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(MergeAction node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AuditSpecificationDetail node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(BrokerPriorityParameter node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateBrokerPriorityStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterBrokerPriorityStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(FailoverModeReplicaOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AvailabilityModeReplicaOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(LiteralReplicaOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AvailabilityReplicaOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AvailabilityReplica node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterAvailabilityGroupStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateAvailabilityGroupStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(PrimaryRoleReplicaOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AvailabilityGroupStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServerConfigurationSetSoftNumaStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServerConfigurationHadrClusterOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServerConfigurationSetHadrClusterStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServerConfigurationFailoverClusterPropertyOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServerConfigurationSetFailoverClusterPropertyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServerConfigurationDiagnosticsLogMaxSizeOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServerConfigurationDiagnosticsLogOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServerConfigurationSoftNumaOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServerConfigurationSetDiagnosticsLogStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SecondaryRoleReplicaOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(LiteralAvailabilityGroupOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TemporalClause node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SelectiveXmlIndexPromotedPath node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(WithinGroupClause node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(WindowDelimiter node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(WindowFrameClause node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateColumnStoreIndexStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DiskStatementOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AvailabilityGroupOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DiskStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropFederationStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterFederationStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateFederationStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropAvailabilityGroupStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterAvailabilityGroupFailoverOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterAvailabilityGroupFailoverAction node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterAvailabilityGroupAction node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(UseFederationStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(BackupRestoreMasterKeyStatementBase node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServerConfigurationBufferPoolExtensionSizeOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServerConfigurationBufferPoolExtensionOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TargetDeclaration node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(EventDeclarationCompareFunctionParameter node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SourceDeclaration node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(EventDeclarationSetParameter node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(EventDeclaration node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateEventSessionStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(EventSessionStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SessionOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(EventSessionObjectName node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterCryptographicProviderStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateCryptographicProviderStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropFullTextStopListStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(FullTextStopListAction node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterFullTextStopListStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateFullTextStopListStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropBrokerPriorityStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropCryptographicProviderStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServerConfigurationBufferPoolExtensionContainerOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(EventRetentionSessionOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(LiteralSessionOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServerConfigurationSetBufferPoolExtensionStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ProcessAffinityRange node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServerConfigurationStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CellsPerObjectSpatialIndexOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(GridParameter node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(GridsSpatialIndexOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(BoundingBoxParameter node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(MemoryPartitionSessionOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(BoundingBoxSpatialIndexOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SpatialIndexOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateSpatialIndexStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterResourceGovernorStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropEventSessionStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterEventSessionStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(OnOffSessionOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(MaxDispatchLatencySessionOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SpatialIndexRegularOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(BackupCertificateStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(OnOffDialogOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ScalarExpressionDialogOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(HavingClause node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(OutputIntoClause node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(OutputClause node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(GroupingSetsGroupingSpecification node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(GrandTotalGroupingSpecification node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(RollupGroupingSpecification node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CubeGroupingSpecification node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(IdentityFunctionCall node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CompositeGroupingSpecification node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(GroupingSpecification node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(GroupByClause node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExpressionWithSortOrder node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(GraphMatchExpression node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(GraphMatchPredicate node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(BooleanIsNullExpression node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(BooleanBinaryExpression node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExpressionGroupingSpecification node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(BooleanComparisonExpression node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(JoinParenthesisTableReference node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(JoinTableReference node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ChangeTableVersionTableReference node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ChangeTableChangesTableReference node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DataModificationTableReference node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TableReferenceWithAliasAndColumns node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TableReferenceWithAlias node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TableReference node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SelectSetVariable node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(OrderByClause node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SelectStarExpression node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SelectElement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(FromClause node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(QueryParenthesisExpression node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(QueryExpression node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(OdbcQualifiedJoinTableReference node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(QualifiedJoin node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SelectScalarExpression node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(BooleanTernaryExpression node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(BooleanParenthesisExpression node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(BooleanExpression node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreationDispositionKeyOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ProviderKeyNameKeyOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(IdentityValueKeyOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlgorithmKeyOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(KeySourceKeyOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(KeyOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateSymmetricKeyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterSymmetricKeyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SymmetricKeyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AuthenticationPayloadOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(RolePayloadOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CharacterSetPayloadOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SchemaPayloadOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SessionTimeoutPayloadOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(LiteralPayloadOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(LoginTypePayloadOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(EncryptionPayloadOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(BooleanNotExpression node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(FullTextCatalogStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(OnOffFullTextCatalogOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ScalarExpression node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TableSampleClause node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(UnqualifiedJoin node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(UnpivotedTableReference node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(PivotedTableReference node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ComputeFunction node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ComputeClause node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(FullTextCatalogOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(GlobalFunctionTableReference node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(BinaryExpression node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ServiceContract node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServiceStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateServiceStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterCreateServiceStatementBase node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterFullTextCatalogStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateFullTextCatalogStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(BuiltInFunctionTableReference node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(EnabledDisabledPayloadOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TopRowFilter node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(UnaryExpression node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropQueueStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropMessageTypeStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropEndpointStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropContractStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(RevertStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterLoginAddDropCredentialStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterLoginEnableDisableStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropRemoteServiceBindingStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterLoginOptionsStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(PasswordAlterPrincipalOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AsymmetricKeyCreateLoginSource node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CertificateCreateLoginSource node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(WindowsCreateLoginSource node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(IdentifierPrincipalOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(LiteralPrincipalOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(OnOffPrincipalOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterLoginStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(PrincipalOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropRouteStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SignatureStatementBase node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DialogOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(BeginDialogStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(BeginConversationTimerStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterServiceMasterKeyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterAsymmetricKeyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterSchemaStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(WaitForSupportedStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropServiceStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SendStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(GetConversationGroupStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(MoveConversationStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(EndConversationStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExecuteAsStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropEventNotificationStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropSignatureStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AddSignatureStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ReceiveStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(OffsetClause node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(PasswordCreateLoginSource node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateLoginStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropMasterKeyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropUserStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropTypeStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropRoleStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropLoginStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropFullTextIndexStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropFullTextCatalogStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropSymmetricKeyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropApplicationRoleStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropAggregateStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropSynonymStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropPartitionSchemeStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropPartitionFunctionStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(VariableMethodCallTableReference node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(VariableTableReference node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(BinaryQueryExpression node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropAssemblyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateLoginSource node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropAsymmetricKeyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropCredentialStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropSearchPropertyListStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropSearchPropertyListAction node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AddSearchPropertyListAction node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SearchPropertyListAction node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterSearchPropertyListStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateSearchPropertyListStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterColumnAlterFullTextIndexAction node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropCertificateStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AddAlterFullTextIndexAction node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SetSearchPropertyListAlterFullTextIndexAction node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SetStopListAlterFullTextIndexAction node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SimpleAlterFullTextIndexAction node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterFullTextIndexAction node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterFullTextIndexStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterPartitionSchemeStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterPartitionFunctionStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropAlterFullTextIndexAction node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterTableSwitchStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SizeFileDeclarationOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CompressionDelayIndexOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ForClause node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(OdbcLiteral node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(LiteralRange node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(StatementWithCtesAndXmlNamespaces node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ValueExpression node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(VariableReference node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(UserDefinedTypePropertyAccess node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(OptionValue node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(FullTextPredicate node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(OnOffOptionValue node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(InPredicate node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(LiteralOptionValue node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(GlobalVariableExpression node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(LikePredicate node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(IdentifierOrValueExpression node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExistsPredicate node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(IdentifierOrScalarExpression node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SubqueryComparisonPredicate node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ParenthesisExpression node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(InlineDerivedTable node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ColumnReferenceExpression node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(QueryDerivedTable node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(MaxLiteral node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DefaultLiteral node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(BrowseForClause node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(IdentifierLiteral node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(OptimizeForOptimizerHint node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(RowValue node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ForceSeekTableHint node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TableHintsOptimizerHint node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(UpdateCall node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TSEqualCall node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(LiteralOptimizerHint node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(PrimaryExpression node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(OptimizerHint node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(Literal node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(NextValueForExpression node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(UpdateForClause node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(NumericLiteral node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(JsonForClauseOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(RealLiteral node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(JsonForClause node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(MoneyLiteral node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(XmlForClauseOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(BinaryLiteral node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(StringLiteral node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(XmlForClause node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(NullLiteral node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ReadOnlyForClause node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(IntegerLiteral node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExecuteInsertSource node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(LiteralTableHint node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SequenceOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropColumnMasterKeyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DataTypeReference node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ColumnEncryptionKeyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateColumnEncryptionKeyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TableValuedFunctionReturnType node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterColumnEncryptionKeyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(FunctionReturnType node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropColumnEncryptionKeyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(WithCtesAndXmlNamespaces node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ColumnEncryptionKeyValue node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ColumnEncryptionKeyValueParameter node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CommonTableExpression node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ColumnMasterKeyNameParameter node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(XmlNamespacesAliasElement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ColumnEncryptionAlgorithmNameParameter node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(XmlNamespacesDefaultElement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(EncryptedValueParameter node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalTableStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(XmlNamespacesElement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalTableOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(XmlNamespaces node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalTableLiteralOrIdentifierOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExecuteAsFunctionOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ParameterizedDataTypeReference node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ColumnMasterKeyPathParameter node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SqlDataTypeReference node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ColumnMasterKeyStoreProviderNameParameter node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(IndexTableHint node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DataTypeSequenceOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TableHint node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ScalarExpressionSequenceOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SchemaObjectFunctionTableReference node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateSequenceStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterSequenceStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(NamedTableReference node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropSequenceStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SecurityPolicyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SequenceStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DeclareTableVariableBody node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TableDefinition node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SecurityPolicyOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SelectFunctionReturnType node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateSecurityPolicyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterSecurityPolicyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ScalarFunctionReturnType node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropSecurityPolicyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(XmlDataTypeReference node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateColumnMasterKeyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(UserDataTypeReference node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ColumnMasterKeyParameter node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SecurityPredicateAction node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SelectInsertSource node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(UseHintList node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ValuesInsertSource node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(LeftFunctionCall node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(LabelStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(UserDefinedTypeCallTarget node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(MultiPartIdentifier node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SchemaObjectName node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(MultiPartIdentifierCallTarget node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ChildObjectName node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExpressionCallTarget node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ProcedureParameter node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CallTarget node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TransactionStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(WhileStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(FunctionCall node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AtTimeZoneCall node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(UpdateDeleteSpecificationBase node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TryCastCall node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DeleteSpecification node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CastCall node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(InsertSpecification node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TryParseCall node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(RightFunctionCall node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(GoToStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(PartitionFunctionCall node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(IdentifierAtomicBlockOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AtomicBlockOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(OnOffAtomicBlockOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(BeginEndAtomicBlockStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(BreakStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(BeginEndBlockStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ColumnWithSortOrder node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterFunctionStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(OdbcConvertSpecification node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExtractFromExpression node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ContinueStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(OdbcFunctionCall node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateDefaultStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ScalarSubquery node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateFunctionStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateOrAlterFunctionStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ParameterlessCall node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateRuleStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(OverClause node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DeclareVariableElement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SaveTransactionStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ParseCall node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(UpdateSpecification node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateSchemaStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(FullTextTableReference node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(GrantStatement80 node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(IIfCall node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DenyStatement80 node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CoalesceExpression node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(RevokeStatement80 node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SecurityElement80 node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(NullIfExpression node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CommandSecurityElement80 node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SearchedCaseExpression node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(PrivilegeSecurityElement80 node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SecurityStatementBody80 node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SimpleCaseExpression node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SecurityUserClause80 node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CaseExpression node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SqlCommandIdentifier node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SearchedWhenClause node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SetClause node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SimpleWhenClause node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AssignmentSetClause node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(FunctionCallSetClause node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(WhenClause node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(InsertSource node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(VariableValuePair node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(Privilege80 node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalTableDistributionOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SecurityPrincipal node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SecurityTargetObjectName node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TryConvertCall node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(WaitForStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ConvertCall node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ReadTextStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SchemaDeclarationItemOpenjson node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(UpdateTextStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(WriteTextStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SchemaDeclarationItem node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TextModificationStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AdHocTableReference node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(LineNoStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SemanticTableReference node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(OpenQueryTableReference node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(GrantStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(BulkOpenRowset node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DenyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(InternalOpenRowset node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(RevokeStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(OpenRowsetTableReference node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterAuthorizationStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(Permission node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(OpenJsonTableReference node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SecurityTargetObject node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(OpenXmlTableReference node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SecurityStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalTableRejectTypeOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SchemaObjectNameOrValueExpression node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(LiteralAtomicBlockOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateProcedureStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ResultColumnDefinition node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropExternalDataSourceStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExecuteAsTriggerOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(RemoteDataArchiveAlterTableOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AssemblyOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterExternalDataSourceStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(InlineResultSetDefinition node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateOrAlterProcedureStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExecutableProcedureReference node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterTableSetStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(RemoteDataArchiveDatabaseOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TableOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(OnOffAssemblyOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ProcedureReference node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExecuteParameter node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(RemoteDataArchiveDatabaseSetting node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalDataSourceLiteralOrIdentifierOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ResultSetDefinition node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterTableFileTableNamespaceStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(PermissionSetAssemblyOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExecutableStringList node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(MethodSpecifier node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(RemoteDataArchiveDbServerSetting node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalDataSourceOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TriggerObject node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateExternalDataSourceStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterAssemblyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(RemoteDataArchiveTableOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalFileFormatStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateExternalFileFormatStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(FileTableDirectoryTableOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateTriggerStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(FileTableCollateFileNameTableOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateOrAlterTriggerStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropExternalFileFormatStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExecuteContext node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalFileFormatContainerOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(FileTableConstraintNameTableOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TriggerStatementBody node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalFileFormatUseDefaultTypeOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(FileStreamOnTableOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AssemblyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExecutableEntity node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExecuteSpecification node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterTriggerStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(MemoryOptimizedTableOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalFileFormatLiteralOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(Microsoft.SqlServer.TransactSql.ScriptDom.Identifier node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateAssemblyStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(LockEscalationTableOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalFileFormatOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DurabilityTableOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SchemaObjectResultSetDefinition node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TriggerAction node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterProcedureStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ProcedureReferenceName node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ResultSetsExecuteOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalDataSourceStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(TriggerOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterTableAddTableElementStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ProcedureStatementBodyBase node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(RemoteDataArchiveDbFederatedServiceAccountSetting node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AddFileSpec node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateViewStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExecuteAsProcedureOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateXmlSchemaCollectionStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(FunctionStatementBody node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalTableShardedDistributionPolicy node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateExternalTableStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterTableRebuildStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterXmlSchemaCollectionStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(StatementList node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(FunctionOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalTableReplicatedDistributionPolicy node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(RetentionPeriodDefinition node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalTableRoundRobinDistributionPolicy node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(CreateOrAlterViewStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropXmlSchemaCollectionStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterTableChangeTrackingModificationStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterTableConstraintModificationStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(RemoteDataArchiveDbCredentialSetting node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(SystemVersioningTableOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AssemblyName node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ViewStatementBody node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExternalTableDistributionPolicy node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ViewOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ProcedureStatementBody node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterTableAlterPartitionStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ProcedureOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(DropExternalTableStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AdHocDataSource node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterViewStatement node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(ExecuteOption node) { node.AcceptChildren(this); }
        public override void ExplicitVisit(AlterTableStatement node) { node.AcceptChildren(this); }

    }
}
