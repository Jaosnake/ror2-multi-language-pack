# Privacidade dos pacotes

## Regra imutável

Nenhum arquivo do projeto pode conter dados do computador particular de quem contribui. Essa regra vale desde a criação do arquivo, em todas as pastas e branches, inclusive testes, originais de trabalho, backups, `main` e pacotes de envio. Abrange qualquer destino, inclusive GitHub, Thunderstore, anexos e armazenamento local do projeto.

É proibido gravar caminhos absolutos de uma máquina, nomes de usuários ou computadores, perfis locais, endereços privados, credenciais e outros dados particulares. Isso vale para o conteúdo, os nomes dos arquivos e os metadados, inclusive campos invisíveis na interface.

- Use caminhos relativos, variáveis de ambiente e referências genéricas na documentação, nas configurações, nos projetos e nos scripts. Passe os valores reais ao processo em tempo de execução e mantenha-os fora dos arquivos e da saída compartilhável.
- Inspecione DLLs e EXEs após compilar. Verifique o caminho de PDB no CodeView, informações de depuração, recursos embutidos e strings. Configure o build para omitir esses caminhos ou usar nomes virtuais. Confira o binário produzido.
- Inspecione cada entrada de qualquer ZIP, inclusive arquivos compactados dentro dele, comentários, nomes, metadados de imagens, fontes e recursos. Aplique a mesma verificação a fontes, MDs, logs, relatórios e capturas antes de salvar, anexar, instalar para teste ou publicar.
- Havendo ocorrência ou item ilegível, interrompa a distribuição. Separe o material afetado fora do projeto e de pastas compartilhadas. Corrija apenas os arquivos cuja manutenção é do projeto e verifique novamente a cópia final. Para binários oficiais ou de terceiros, preserve o original e resolva com o fornecedor.
- Para material já publicado, a ação deve abranger a cópia hospedada, seus assets de release e, no GitHub, o histórico acessível. Um arquivo limpo na versão mais recente não retira a exposição de versões anteriores. Registre a ocorrência com o nome do pacote, a versão e o componente, sempre omitindo o dado privado.
