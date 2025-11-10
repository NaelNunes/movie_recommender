import React, { useState } from 'react'
import axios from 'axios'

type Movie = {
  id: number
  title: string
  year?: string
  rating?: number
  genre?: string
  posterPath?: string | null
  backdropPath?: string | null
  description?: string
}

export default function App(): JSX.Element {
  const [prompt, setPrompt] = useState('')
  const [results, setResults] = useState<Movie[]>([])
  const [loading, setLoading] = useState(false)
  const [selectedMovie, setSelectedMovie] = useState<Movie | null>(null)

  const onGenerate = async () => {
    if (!prompt.trim()) return
    setLoading(true)

    try {
      const response = await axios.post<Movie[]>(
        'http://localhost:5271/api/Movies/search',
        { prompt, limit: 5 }
      )
      setResults(response.data)
    } catch (error) {
      console.error('Erro ao buscar filmes:', error)
      alert('Erro ao buscar filmes. Verifique se o servidor está rodando.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen bg-[linear-gradient(180deg,#050613_0%,#071026_100%)] text-white flex flex-col items-center justify-start px-6 py-12">
      <div className="w-full max-w-6xl">
        {/* Header */}
        <header className="text-center mb-8">
          <h1 className="text-5xl font-extrabold tracking-tight">MovieRecommender</h1>
          <p className="mt-3 text-sm text-slate-300">
            Descreva o que você gosta — eu sugiro filmes.
          </p>
        </header>

        {/* Input */}
        <section className="bg-[rgba(255,255,255,0.02)] border border-slate-700 rounded-2xl p-6 mb-8">
          <label className="block text-sm text-slate-300 mb-2">
            O que você quer assistir?
          </label>
          <textarea
            value={prompt}
            onChange={(e) => setPrompt(e.target.value)}
            placeholder="Ex: ficção científica com ação"
            className="w-full min-h-[120px] bg-transparent border border-slate-700 rounded-md p-3 text-sm placeholder:text-slate-400 resize-none focus:outline-none focus:ring-1 focus:ring-purple-500"
          />
          <div className="mt-4 flex items-center justify-between">
            <span className="text-xs text-slate-400">
              Dica: experimente 'Sci-Fi' ou 'Drama'
            </span>
            <div className="flex gap-2">
              <button
                onClick={() => setPrompt('')}
                className="px-3 py-2 rounded-md border border-slate-700 text-sm hover:bg-slate-700/20 transition"
              >
                Limpar
              </button>
              <button
                onClick={onGenerate}
                disabled={loading}
                className="px-4 py-2 rounded-md bg-gradient-to-r from-[#7c5cff] to-[#00d4ff] text-black font-semibold hover:opacity-90 transition"
              >
                {loading ? 'Gerando...' : 'Pesquisar'}
              </button>
            </div>
          </div>
        </section>

        {/* Horizontal Movie List Centralizada */}
        <div className="w-full">
          <h3 className="text-xl font-bold mb-4 text-center">Recomendações</h3>
          <div className="flex justify-center gap-6 overflow-x-auto py-4">
            {results.length === 0 && (
              <div className="text-sm text-slate-400 text-center">Nenhum resultado ainda. Clique em Pesquisar.</div>
            )}

            {results.map((r) => (
              <div
                key={r.id}
                className="flex-shrink-0 min-w-[180px] relative rounded-xl shadow-lg bg-slate-900 cursor-pointer hover:scale-105 transition-transform"
                onClick={() => setSelectedMovie(r)}
              >
                {r.posterPath ? (
                  <img
                    src={r.posterPath}
                    alt={r.title}
                    className="w-full h-72 object-cover rounded-xl"
                  />
                ) : (
                  <div className="w-full h-72 bg-slate-800 flex items-center justify-center text-xs text-slate-400 rounded-xl">
                    Sem imagem
                  </div>
                )}
                <div className="absolute bottom-0 left-0 right-0 bg-gradient-to-t from-black/80 to-transparent p-2 rounded-b-xl">
                  <h4 className="text-sm font-bold">{r.title}</h4>
                  <p className="text-xs text-slate-300">{r.genre} • {r.year}</p>
                  {r.rating !== undefined && <span className="text-yellow-400 text-xs">⭐ {r.rating.toFixed(1)}</span>}
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Modal */}
      {selectedMovie && (
        <div
          className="fixed inset-0 bg-black/90 flex items-center justify-center z-50 p-4"
          onClick={() => setSelectedMovie(null)}
        >
          <div
            className="bg-slate-900 rounded-xl max-w-4xl w-full overflow-hidden relative shadow-2xl"
            onClick={(e) => e.stopPropagation()}
          >
            {/* Imagem elegante */}
            {selectedMovie.backdropPath || selectedMovie.posterPath ? (
              <div className="relative w-full h-96">
                <img
                  src={selectedMovie.backdropPath ?? selectedMovie.posterPath ?? ''}
                  alt={selectedMovie.title}
                  className="w-full h-full object-cover object-center brightness-90"
                />
                <div className="absolute inset-0 bg-gradient-to-t from-black/70 to-transparent" />
              </div>
            ) : (
              <div className="w-full h-96 bg-slate-800 flex items-center justify-center text-xs text-slate-400">
                Sem imagem
              </div>
            )}

            {/* Conteúdo */}
            <div className="p-6">
              <h2 className="text-3xl font-bold mb-2">{selectedMovie.title}</h2>
              <p className="text-sm text-slate-300 mb-2">{selectedMovie.genre} • {selectedMovie.year}</p>
              {selectedMovie.rating !== undefined && (
                <p className="text-yellow-400 font-semibold mb-4">⭐ {selectedMovie.rating.toFixed(1)}</p>
              )}
              {selectedMovie.description && (
                <p className="text-sm text-slate-300 mb-4">{selectedMovie.description}</p>
              )}
              <button
                onClick={() => setSelectedMovie(null)}
                className="mt-2 px-4 py-2 bg-gradient-to-r from-[#7c5cff] to-[#00d4ff] text-black font-semibold rounded-md hover:opacity-90 transition"
              >
                Fechar
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
