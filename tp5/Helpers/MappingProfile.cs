using AutoMapper;
using MoviesApi.Dtos;
using MoviesApi.Models;

namespace MoviesApi.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Genre, GenreDto>();

            CreateMap<Movie, MovieDetailsDto>()
                .ForMember(
                    dest => dest.GenreName,
                    opt => opt.MapFrom(src => src.Genre.Name));
        }
    }
}
