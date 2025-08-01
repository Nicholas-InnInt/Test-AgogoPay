﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Execution;
using GraphQL.SystemTextJson;
using GraphQL.Types;
using GraphQL.Validation;
using GraphQLParser.Exceptions;
using Neptune.NsPay.Test.Base;
using Shouldly;

namespace Neptune.NsPay.GraphQL.Tests
{
    public class GraphQLTestBase<TSchema> : GraphQLTestBase<TSchema, GraphQLDocumentBuilder>
        where TSchema : ISchema
    {

    }

    public class GraphQLTestBase<TSchema, TDocumentBuilder> : AppTestBase<NsPayGraphQLTestModule>
        where TSchema : ISchema
        where TDocumentBuilder : IDocumentBuilder, new()
    {
        public TSchema Schema;

        public IDocumentExecuter Executer { get; }

        public IGraphQLTextSerializer Writer { get; }

        public GraphQLTestBase()
        {
            Schema = Resolve<TSchema>();

            Executer = new DocumentExecuter(new TDocumentBuilder(), new DocumentValidator());

            Writer = new GraphQLSerializer(indent: true);
        }

        public async Task<ExecutionResult> AssertQuerySuccessAsync(
            string query,
            string expectedResult,
            Inputs inputs = null,
            object root = null,
            Dictionary<string, object> userContext = null,
            CancellationToken cancellationToken = default,
            IEnumerable<IValidationRule> rules = null)
        {
            var queryResult = ParseQueryResult(expectedResult);

            return await AssertQueryAsync(query, queryResult, inputs, root, userContext, cancellationToken, rules);
        }

        public async Task<ExecutionResult> AssertQueryWithErrorsAsync(
            string query,
            string expectedResult,
            Inputs inputs = null,
            object root = null,
            Dictionary<string, object> userContext = null,
            CancellationToken cancellationToken = default,
            int expectedErrorCount = 0,
            bool renderErrors = false)
        {
            var queryResult = ParseQueryResult(expectedResult);

            return await AssertQueryIgnoreErrorsAsync(
                query,
                queryResult,
                inputs,
                root,
                userContext,
                cancellationToken,
                expectedErrorCount,
                renderErrors);
        }

        public async Task<ExecutionResult> AssertQueryIgnoreErrorsAsync(
            string query,
            ExecutionResult expectedExecutionResult,
            Inputs inputs = null,
            object root = null,
            Dictionary<string, object> userContext = null,
            CancellationToken cancellationToken = default,
            int expectedErrorCount = 0,
            bool renderErrors = false)
        {
            var executionResult = await Executer.ExecuteAsync(options =>
            {
                options.Schema = Schema;
                options.Query = query;
                options.Root = root;
                options.Variables = inputs;
                options.UserContext = userContext;
                options.CancellationToken = cancellationToken;
            });

            var actualResult = Writer.Serialize(renderErrors
                ? executionResult
                : new ExecutionResult
                {
                    Data = executionResult.Data
                });

            var expectedResult = Writer.Serialize(expectedExecutionResult);

            actualResult.ShouldBe(expectedResult);

            if (executionResult.Errors == null)
            {
                executionResult.Errors = new ExecutionErrors();
            }

            executionResult.Errors.Count().ShouldBe(expectedErrorCount);

            return executionResult;
        }

        public async Task<ExecutionResult> AssertQueryAsync(
            string query,
            ExecutionResult expectedExecutionResult,
            Inputs inputs,
            object root,
            Dictionary<string, object> userContext = null,
            CancellationToken cancellationToken = default,
            IEnumerable<IValidationRule> rules = null)
        {
            var executionResult = await Executer.ExecuteAsync(options =>
            {
                options.Schema = Schema;
                options.Query = query;
                options.Root = root;
                options.Variables = inputs;
                options.UserContext = userContext;
                options.CancellationToken = cancellationToken;
                options.ValidationRules = rules;
            });

            var actualResult = Writer.Serialize(executionResult);

            var expectedResult = Writer.Serialize(expectedExecutionResult.Data);

            string additionalInfo = null;

            if (executionResult.Errors?.Any() == true)
            {
                additionalInfo = string.Join(Environment.NewLine, executionResult.Errors
                    .Where(x => x.InnerException is GraphQLSyntaxErrorException)
                    .Select(x => x.InnerException.Message));
            }

            actualResult.ShouldBe(expectedResult, additionalInfo);

            return executionResult;
        }

        public ExecutionResult ParseQueryResult(string result, ExecutionErrors errors = null)
        {
            object data = null;

            if (!string.IsNullOrWhiteSpace(result))
            {
                data = JsonNode.Parse(result);
            }

            return new ExecutionResult
            {
                Data = data,
                Errors = errors
            };
        }
    }
}