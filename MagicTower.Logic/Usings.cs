//@CodeCopy

#if IDINT_ON
global using IdType = System.Int32;
#elif IDLONG_ON
global using IdType = System.Int64;
#elif IDGUID_ON
global using IdType = System.Guid;
#else
global using IdType = System.Int32;
#endif
global using Common = MagicTower.Common;
global using CommonEnums = MagicTower.Common.Enums;
global using CommonContracts = MagicTower.Common.Contracts;
global using CommonModels = MagicTower.Common.Models;
global using CommonModules = MagicTower.Common.Modules;
global using MagicTower.Common.Extensions;
global using System.ComponentModel.DataAnnotations;
global using System.ComponentModel.DataAnnotations.Schema;
global using Microsoft.EntityFrameworkCore;
global using Validator = MagicTower.Common.Modules.Validations.Validator;

