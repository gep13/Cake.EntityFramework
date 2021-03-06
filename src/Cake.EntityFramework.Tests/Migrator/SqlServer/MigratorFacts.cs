﻿using System;
using System.Linq;
using Cake.EntityFramework.Interfaces;
using Cake.EntityFramework.Migrator;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using Cake.EntityFramework.Tests.Fixtures;

namespace Cake.EntityFramework.Tests.Migrator.SqlServer
{
    public class MigratorFacts
    {
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly ITestOutputHelper _logHelper;

        private readonly ILogger _mockLogger;

        private readonly string _instanceString = SqlServerFactConstants.InstanceConnectionString;

        private IEfMigrator Migrator => new EfMigrator(SqlServerFactConstants.DdlPath, SqlServerFactConstants.ConfigName, SqlServerFactConstants.AppConfig,
            _instanceString, SqlServerFactConstants.ConnectionProvider, _mockLogger, false);

        public MigratorFacts(ITestOutputHelper logHelper)
        {
            _logHelper = logHelper;
            _mockLogger = new MockLogger(logHelper);

            _logHelper.WriteLine($"Using connectionString: {_instanceString}");
        }

        [Fact]
        public void When_no_error_on_construct_ready_should_be_true()
        {
            var migrator = Migrator;

            // Act
            var result = migrator.Ready;

            // Assert
            result.Should().BeTrue();
        }
        
        [Fact]
        public void When_migrating_fails_return_false()
        {
            var migrator = Migrator;

            // Act
            Action action = () => migrator.MigrateToLatest();

            // Assert
            action.ShouldThrow<Exception>();
        }

        [Fact]
        public void When_migrating_succeeds_return_true()
        {
            var migrator = Migrator;
            var migrations = migrator.GetLocalMigrations().ToList();
            var firstMigration = migrations.First();

            // Act
            var success = migrator.MigrateTo(firstMigration);

            // Assert
            success.Should().BeTrue();
        }

        [Fact]
        public void When_committed_rollback_should_throw()
        {
            var migrator = Migrator;
            var lastGoodMigration = migrator.GetLocalMigrations().Skip(1).First();

            // Act
            migrator.MigrateTo(lastGoodMigration);
            migrator.Commit();

            Action action = () => migrator.Rollback();

            // Assert
            action.ShouldThrow<Exception>();
        }


        [Fact(DisplayName = "Generate Script for Latest")]
        public void When_generating_script_return_true()
        {
            var migrator = Migrator;

            // Act
            var scriptResult = Migrator.GenerateScriptForLatest();

            // Assert
            scriptResult.Should().NotBeNullOrEmpty();
        }
    }
}