using System;
using System.Collections.Generic;
using System.Linq;
using API.Dtos;
using API.Errors;
using API.Helpers;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class ProdutosController : BaseApiController
    {
        private readonly IGenericRepository<Produto> _produtosRepo;
        private readonly IGenericRepository<ProdutoCategoria> _produtoCategoriaRepo;
        private readonly IGenericRepository<ProdutoMarca> _produtoMarcaRepo;
        private readonly IMapper _mapper;

        public ProdutosController(IGenericRepository<Produto> produtosRepo,
        IGenericRepository<ProdutoCategoria> produtoCategoriaRepo, IGenericRepository<ProdutoMarca>
        produtoMarcaRepo, IMapper mapper)
        {
            _mapper = mapper;
            _produtoMarcaRepo = produtoMarcaRepo;
            _produtoCategoriaRepo = produtoCategoriaRepo;
            _produtosRepo = produtosRepo;

        }

        [HttpGet]
        public async Task<ActionResult<Pagination<ProdutoToReturnDto>>> GetProdutos(
            [FromQuery]ProdutoSpecParams produtoParams)
        {
            var spec = new ProdutosWithMarcasAndCategoriasSpecification(produtoParams);

            var CountSpec = new ProdutosWithFiltersForCountSpecification(produtoParams);

            var totalItems = await _produtosRepo.CountAsync(CountSpec);

            var produtos = await _produtosRepo.ListAsync(spec);

            var data = _mapper
                .Map<IReadOnlyList<Produto>, IReadOnlyList<ProdutoToReturnDto>>(produtos);

            return Ok(new Pagination<ProdutoToReturnDto>(produtoParams.PageIndex,
            produtoParams.PageSize, totalItems, data));
        }

        //pegar pessoa pelo Id
        [HttpGet("{Id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProdutoToReturnDto>> GetProduto(int Id)
        {
            var spec = new ProdutosWithMarcasAndCategoriasSpecification(Id);

            var produto = await _produtosRepo.GetEntityWithSpec(spec);

            if (produto == null) return NotFound(new ApiResponse(404));

            return _mapper.Map<Produto, ProdutoToReturnDto>(produto);
        }

        [HttpGet("categorias")]
        public async Task<ActionResult<IReadOnlyList<ProdutoCategoria>>> GetProdutoCategorias()
        {
            return Ok(await _produtoCategoriaRepo.ListAllAsync());
        }

        [HttpGet("marcas")]
        public async Task<ActionResult<IReadOnlyList<ProdutoMarca>>> GetProdutoMarcas()
        {
            return Ok(await _produtoMarcaRepo.ListAllAsync());
        }
    }
}