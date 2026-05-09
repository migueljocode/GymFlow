global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Design;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;

global using System.Linq.Expressions;

global using GymFlow.Models.Entities;
global using GymFlow.Models.Base;
global using GymFlow.Models.Enums;
global using GymFlow.Dal.Context;
global using GymFlow.Dal.Repositories.Interfaces;
global using GymFlow.Dal.Repositories.Implementations;
global using GymFlow.Dal.Factories;
global using GymFlow.Dal.Seed.Constants;
global using GymFlow.Dal.Seed.Data;
global using GymFlow.Dal.Seed.Extensions;

global using Bogus;
