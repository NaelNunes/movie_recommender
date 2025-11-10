// Backup of the corrupted App.tsx content (moved out of src so TypeScript won't compile it)
// If you need any snippets from here, you can find them in this backup file.

/*
Paste of original corrupted content follows for reference. Do not import this file.

*/

import React, { useState } from 'react'

type Movie = {
  id: number
  title: string
  year: string
  rating: number
  genre: string
}

const SAMPLE: Movie[] = [
  { id: 1, title: 'Neon Runners', year: '2024', rating: 8.7, genre: 'Sci-Fi' },
  { id: 2, title: 'Digital Awakening', year: '2023', rating: 9.1, genre: 'Sci-Fi' },
  { id: 3, title: 'Cosmic Odyssey', year: '2024', rating: 8.9, genre: 'Adventure' },
]

export function BackupApp(): JSX.Element {
  const [prompt, setPrompt] = useState('')
  const [results, setResults] = useState<Movie[]>([])
  const [loading, setLoading] = useState(false)

  const onGenerate = () => {
    if (!prompt.trim()) return
    setLoading(true)
    setTimeout(() => {
      const q = prompt.toLowerCase()
      const filtered = SAMPLE.filter(m => m.title.toLowerCase().includes(q) || m.genre.toLowerCase().includes(q))
      setResults(filtered.length ? filtered : SAMPLE)
      setLoading(false)
    }, 600)
  }

  return (
    <div className="min-h-screen bg-[linear-gradient(180deg,#050613_0%,#071026_100%)] text-white flex items-center justify-center px-6">
      <div className="max-w-4xl w-full">
        <header className="text-center mb-8">
          <h1 className="text-4xl font-extrabold tracking-tight">CineAI</h1>
          <p className="mt-2 text-sm text-slate-300">Descreva o que você gosta — eu sugiro filmes.</p>
        </header>

        <section className="bg-[rgba(255,255,255,0.02)] border border-slate-700 rounded-2xl p-6">
          <label className="block text-sm text-slate-300 mb-2">O que você quer assistir?</label>
          <textarea
            value={prompt}
            onChange={(e) => setPrompt(e.target.value)}
            placeholder="Ex: ficção científica com ação"
            className="w-full min-h-[120px] bg-transparent border border-slate-700 rounded-md p-3 text-sm placeholder:text-slate-400 resize-none"
          />
          <div className="mt-4 flex items-center justify-between">
            <span className="text-xs text-slate-400">Dica: experimente 'Sci-Fi' ou 'Drama'</span>
            <div className="flex gap-2">
              <button onClick={() => setPrompt('')} className="px-3 py-2 rounded-md border border-slate-700 text-sm">Limpar</button>
              <button onClick={onGenerate} disabled={loading} className="px-4 py-2 rounded-md bg-gradient-to-r from-[#7c5cff] to-[#00d4ff] text-black font-semibold">{loading ? 'Gerando...' : 'Pesquisar'}</button>
            </div>
          </div>
        </section>

        <div className="mt-6">
          <h3 className="text-lg font-medium">Recomendações</h3>
          <div className="mt-3 grid grid-cols-1 sm:grid-cols-2 gap-4">
            {results.length === 0 && <div className="text-sm text-slate-400">Nenhum resultado ainda. Clique em Pesquisar.</div>}
            {results.map(r => (
              <div key={r.id} className="p-3 bg-[rgba(255,255,255,0.02)] border border-slate-700 rounded-md">
                <div className="flex items-center justify-between">
                  <div>
                    <div className="font-semibold">{r.title}</div>
                    <div className="text-xs text-slate-400">{r.genre} • {r.year}</div>
                  </div>
                  <div className="text-sm bg-slate-800 px-2 py-1 rounded">⭐ {r.rating}</div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  )
}

// -- The original corrupted/concatenated content that was causing duplicate declarations is intentionally omitted here
