using Main.Inclusion.Validated.Result;
using Main.Sql.SqlServer.Validator;
using Main.Sql.SqlServer.Visitor.Known;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Sql.SqlServer.Visitor
{
    public class StatementVisitor : ISqlStatementVisitor<IValidationResult2>
    {
        public const string VariableTypeName = "TOKEN_VARIABLE";
        public const string SqlServerSpecificVariablePrefix = "@@";
        public const string SqlServerVariablePrefix = "@";
        public const string SqlServerTempTablePrefix1 = "##";
        public const string SqlServerTempTablePrefix2 = "#";

        private readonly ISqlValidator _sqlValidator;

        private readonly List<IKnownVariable> _knownVariables;

        private StatementVisitor(
            StatementVisitor visitor
            )
        {
            if (visitor == null)
            {
                throw new ArgumentNullException(nameof(visitor));
            }

            _sqlValidator = visitor._sqlValidator;
            _knownVariables = new List<IKnownVariable>(visitor._knownVariables);
        }

        public StatementVisitor(
            ISqlValidator sqlValidator
            )
        {
            if (sqlValidator == null)
            {
                throw new ArgumentNullException(nameof(sqlValidator));
            }

            _sqlValidator = sqlValidator;
            _knownVariables = new List<IKnownVariable>();
        }


        public IValidationResult2 Visit(SqlAlterFunctionStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlAlterLoginStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlAlterProcedureStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlAlterTriggerStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlAlterViewStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlBackupCertificateStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlBackupDatabaseStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlBackupLogStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlBackupMasterKeyStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlBackupServiceMasterKeyStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlBackupTableStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlBreakStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlCommentStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlCompoundStatement statement)
        {
            if (statement == null)
            {
                throw new ArgumentNullException(nameof(statement));
            }

            foreach (var innerStatement in statement.Statements)
            {
                var result = innerStatement.Accept(this);
                if (!result.IsSuccess)
                {
                    return
                        result.WithNewFullSqlBody(statement.Sql);
                }
            }

            return
                ValidationResult.Success(statement.Sql, statement.Sql);
        }

        public IValidationResult2 Visit(SqlContinueStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlCreateFunctionStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlCreateIndexStatement statement)
        {
            return ExecuteDefaultProcessing(statement);
        }

        public IValidationResult2 Visit(SqlCreateLoginFromAsymKeyStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlCreateLoginFromCertificateStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlCreateLoginFromWindowsStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlCreateLoginWithPasswordStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlCreateProcedureStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlCreateRoleStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlCreateSchemaStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlCreateSynonymStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlCreateTableStatement statement)
        {
            //вот этот тип запроса (создание временной таблицы) как раз можно провалидировать
            //например может быть опечатка в типе данных, и эта ошибка будет найдена

            return ExecuteDefaultProcessing(statement);
        }

        public IValidationResult2 Visit(SqlCreateTriggerStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlCreateUserDefinedDataTypeStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlCreateUserDefinedTableTypeStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlCreateUserDefinedTypeStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlCreateUserFromAsymKeyStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlCreateUserFromCertificateStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlCreateUserWithImplicitAuthenticationStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlCreateUserFromLoginStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlCreateUserFromExternalProviderStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlCreateUserStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlCreateUserWithoutLoginStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlCreateViewStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlCursorDeclareStatement statement)
        {
            return ExecuteDefaultProcessing(statement);
        }

        public IValidationResult2 Visit(SqlDBCCStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlDeleteStatement statement)
        {
            IValidationResult2 tempTableVisitReturn;
            if (TryToFoundTempTableReference(statement, out tempTableVisitReturn))
            {
                return tempTableVisitReturn;
            }

            return ExecuteDefaultProcessing(statement);
        }

        public IValidationResult2 Visit(SqlDenyStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlDropAggregateStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlDropDatabaseStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlDropDefaultStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlDropFunctionStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlDropLoginStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlDropProcedureStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlDropRuleStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlDropSchemaStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlDropSecurityPolicyStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlDropSequenceStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlDropSynonymStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlDropTableStatement statement)
        {
            //вот этот тип запроса (удаление временной таблицы) как раз можно провалидировать
            //тут не надо искать иеднтификаторов временных таблиц

            return ExecuteDefaultProcessing(statement);
        }

        public IValidationResult2 Visit(SqlDropTriggerStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlDropTypeStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlDropUserStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlDropViewStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlExecuteModuleStatement statement)
        {
            return ExecuteDefaultProcessing(statement);
        }

        public IValidationResult2 Visit(SqlExecuteStringStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlGrantStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(
            SqlIfElseStatement statement
            )
        {
            var statementOffset = statement.StartLocation.Offset;

            var trueStatement = statement.TrueStatement;
            var falseStatement = statement.FalseStatement;

            SqlBooleanExpression condition = statement.Condition;

            var sqlBody = condition.FixDuplicates(
                _knownVariables
                );

            sqlBody = string.Format(
                "if {0} print 1;",
                sqlBody
                );

            var conditionResult = ExecuteDefaultProcessing(
                sqlBody
                );
            if (!conditionResult.IsSuccess)
            {
                return conditionResult.WithNewFullSqlBody(statement.Sql);
            }

            var trueResult = trueStatement.Accept(new StatementVisitor(this));
            if (!trueResult.IsSuccess || falseStatement == null)
            {
                return trueResult.WithNewFullSqlBody(statement.Sql);
            }

            var falseResult = falseStatement.Accept(new StatementVisitor(this));
            if (!falseResult.IsSuccess)
            {
                return
                    falseResult.WithNewFullSqlBody(statement.Sql);
            }

            return
                ValidationResult.Success(statement.Sql, statement.Sql);
        }

        public IValidationResult2 Visit(SqlInlineTableVariableDeclareStatement statement)
        {
            var result = ExecuteDefaultProcessing(statement);

            if (result.IsSuccess)
            {
                var newDeclarationTerm = KnownTableVariable.Parse(statement);
                if (newDeclarationTerm != null)
                {
                    _knownVariables.Add(newDeclarationTerm);
                }
            }

            return
                result;
        }

        public IValidationResult2 Visit(SqlInsertStatement statement)
        {
            IValidationResult2 tempTableVisitReturn;
            if (TryToFoundTempTableReference(statement, out tempTableVisitReturn))
            {
                return tempTableVisitReturn;
            }

            return ExecuteDefaultProcessing(statement);
        }

        public IValidationResult2 Visit(SqlMergeStatement statement)
        {
            IValidationResult2 tempTableVisitReturn;
            if (TryToFoundTempTableReference(statement, out tempTableVisitReturn))
            {
                return tempTableVisitReturn;
            }

            var sql = statement.FixDuplicates(_knownVariables) + ";";

            return ExecuteDefaultProcessing(sql);
        }

        public IValidationResult2 Visit(SqlStatement statement)
        {
            return ValidationResult.Success(statement.Sql, statement.Sql);
        }

        public IValidationResult2 Visit(SqlRestoreDatabaseStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlRestoreInformationStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlRestoreLogStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlRestoreMasterKeyStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlRestoreServiceMasterKeyStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlRestoreTableStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlReturnStatement statement)
        {
            return ValidationResult.Success(statement.Sql, statement.Sql);
        }

        public IValidationResult2 Visit(SqlRevokeStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlSelectStatement statement)
        {
            return ExecuteDefaultProcessing(statement);
        }

        public IValidationResult2 Visit(SqlSetAssignmentStatement statement)
        {
            return ExecuteDefaultProcessing(statement);
        }

        public IValidationResult2 Visit(SqlSetStatement statement)
        {
            return ExecuteDefaultProcessing(statement);
        }

        public IValidationResult2 Visit(SqlTryCatchStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlUpdateStatement statement)
        {
            IValidationResult2 tempTableVisitReturn;
            if (TryToFoundTempTableReference(statement, out tempTableVisitReturn))
            {
                return tempTableVisitReturn;
            }

            return ExecuteDefaultProcessing(statement);
        }

        public IValidationResult2 Visit(SqlUseStatement statement)
        {
            return new NotImplementedValidationResult(statement);
        }

        public IValidationResult2 Visit(SqlVariableDeclareStatement statement)
        {
            var result = ExecuteDefaultProcessing(statement);

            if (result.IsSuccess)
            {
                var newDeclarationTerm = KnownVariable.Parse(statement);
                if (newDeclarationTerm != null)
                {
                    _knownVariables.Add(newDeclarationTerm);
                }
            }

            return
                result;
        }

        public IValidationResult2 Visit(SqlWhileStatement statement)
        {
            //проверяем само условие WHILE

            var condition = statement.Condition;

            var sqlBody = condition.FixDuplicates(
                _knownVariables
                );

            sqlBody = string.Format(
                "while( {0} ) print 1;",
                sqlBody
                );

            var conditionResult = ExecuteDefaultProcessing(
                sqlBody
                );
            if (!conditionResult.IsSuccess)
            {
                return conditionResult.WithNewFullSqlBody(statement.Sql);
            }

            //теперь проверяем кишочки условия WHILE

            var trueStatement = statement.TrueStatement;
            if (trueStatement != null)
            {
                var bodyResult = trueStatement.Accept(new StatementVisitor(this));
                if (!bodyResult.IsSuccess)
                {
                    return bodyResult.WithNewFullSqlBody(statement.Sql);
                }
            }

            return ValidationResult.Success(statement.Sql, statement.Sql);
        }



        private bool TryToFoundTempTableReference(
            SqlStatement statement,
            out IValidationResult2 visitResult
            )
        {
            try
            {
                statement.Accept(new CheckForTempTableNameReferenceVisitor());

                visitResult = null;

                return
                    false; //not found
            }
            catch (Exception excp)
            {
                visitResult = ValidationResult.CannotValidate(statement.Sql, statement.Sql, excp.Message);
                return true; //found
            }
        }

        private IValidationResult2 ExecuteDefaultProcessing(
            SqlCodeObject statement
            )
        {
            if (statement == null)
            {
                throw new ArgumentNullException(nameof(statement));
            }

            var sql = statement.FixDuplicates(_knownVariables);

            var result = ExecuteDefaultProcessing(
                sql
                );

            if (!result.IsSuccess)
            {
                result = result.WithNewFullSqlBody(statement.Sql);
            }

            return
                result;
        }

        private IValidationResult2 ExecuteDefaultProcessing(
            string statementSql
            )
        {

            if (statementSql == null)
            {
                throw new ArgumentNullException(nameof(statementSql));
            }

            var declarations = string.Join(Environment.NewLine, _knownVariables.ConvertAll(j => j.ToSqlDeclaration()));

            IValidationResult2 result;

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
    }

}
