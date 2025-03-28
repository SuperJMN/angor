﻿using Angor.Projects.Application.Dtos;
using Angor.Projects.Domain;
using Angor.Projects.Infrastructure.Interfaces;
using CSharpFunctionalExtensions;
using Zafiro.CSharpFunctionalExtensions;
using Amount = Angor.Projects.Domain.Amount;

namespace Angor.Projects.Infrastructure.Impl;

public class ProjectAppService(
    IProjectRepository projectRepository,
    IInvestmentRepository investmentRepository, InvestCommandFactory investmentCommandFactory)
    : IProjectAppService
{
    public Task<Result> Invest(Guid walletId, ProjectId projectId, Amount amount)
    {
        var command = investmentCommandFactory.Create(walletId, projectId, amount);
        return command.Execute();
    }

    public Task<Result<IList<InvestmentDto>>> GetInvestments(ProjectId projectId)
    {
        return investmentRepository.GetByProject(projectId);
    }

    public async Task<IList<ProjectDto>> Latest()
    {
        var projects = await projectRepository.Latest();
        var projectDtos = projects.Select(project => project.ToDto());
        return projectDtos.ToList();
    }

    public Task<Maybe<ProjectDto>> FindById(ProjectId projectId)
    {
        return projectRepository.Get(projectId).Map(project1 => project1.ToDto()).AsMaybe();
    }
}