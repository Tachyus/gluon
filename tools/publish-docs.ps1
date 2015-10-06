# publishes docs/output to gh-pages

git push origin --delete gh-pages
git branch temp
git checkout temp
git add docs/output
git commit -am 'Added generated docs'
git subtree push --prefix docs/output origin gh-pages
git checkout master
git branch -D temp
